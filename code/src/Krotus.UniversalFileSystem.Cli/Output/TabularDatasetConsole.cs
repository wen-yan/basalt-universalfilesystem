using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;

namespace Krotus.UniversalFileSystem.Cli.Output;

enum TabularDatasetConsoleAlignment
{
    Left,
    Center,
    Right
}

class TabularDatasetConsoleAttribute : Attribute
{
    public string? ColumnName { get; set; }
    public TabularDatasetConsoleAlignment Alignment { get; set; } = TabularDatasetConsoleAlignment.Left;
    public string? ToStringMethodName { get; set; }
}

class TabularDatasetConsole : IDatasetConsole
{
    public async ValueTask WriteAsync<T>(IAsyncEnumerable<T> dataset, CancellationToken cancellationToken)
    {
        static string? DefaultToString(object? item, object? value)
        {
            return value?.ToString();
        }

        Type type = typeof(T);
        List<(PropertyInfo Property, TabularDatasetConsoleAttribute? Attribute, Func<object?, object?> GetValueFunc, Func<object?, object?, string?> ToStringFunc)> properties = type.GetProperties()
            .Where(x => !x.IsSpecialName)
            .Where(x => x.CanRead)
            .Where(x => (x.GetGetMethod()?.IsPublic ?? false) && (x.GetGetMethod()?.IsStatic ?? true) == false)
            .Select(x =>
            {
                TabularDatasetConsoleAttribute? attribute = x.GetCustomAttribute<TabularDatasetConsoleAttribute>();
                Func<object?, object?> getValueFunc = item => x.GetGetMethod()?.Invoke(item, null) ?? null;

                Func<object?, object?, string?> toStringFunc = DefaultToString;
                if (attribute?.ToStringMethodName != null)
                {
                    MethodInfo? method = type.GetMethod(attribute.ToStringMethodName);
                    if (method != null)
                    {
                        if (method.IsStatic)
                            toStringFunc = (item, value) => (string?)method.Invoke(null, [value]);
                        else
                            toStringFunc = (item, value) => (string?)method.Invoke(item, [value]);
                    }
                }

                return (Property: x, Attribute: attribute, GetValueFunc: getValueFunc, ToStringFunc: toStringFunc);
            })
            .ToList();

        Table table = new() { Border = TableBorder.Simple };

        // columns
        foreach ((PropertyInfo Property, TabularDatasetConsoleAttribute? Attribute, Func<object?, object?> GetValueFunc, Func<object?, object?, string?> ToStringFunc) property in properties)
        {
            string columnName = property.Attribute?.ColumnName ?? property.Property.Name;
            TabularDatasetConsoleAlignment alignment = property.Attribute?.Alignment ?? TabularDatasetConsoleAlignment.Left;

            TableColumn column = new(columnName)
            {
                Alignment = alignment switch
                {
                    TabularDatasetConsoleAlignment.Left => Justify.Left,
                    TabularDatasetConsoleAlignment.Right => Justify.Right,
                    TabularDatasetConsoleAlignment.Center => Justify.Center,
                    _ => throw new ArgumentOutOfRangeException()
                }
            };
            table.AddColumn(column);
        }

        // rows
        await foreach (T item in dataset.WithCancellation(cancellationToken))
        {
            string[] row = properties.Select(x =>
                {
                    object? value = x.GetValueFunc(item);
                    return x.ToStringFunc(item, value) ?? string.Empty;
                })
                .ToArray();
            table.AddRow(row);
        }

        AnsiConsole.Write(table);
    }
}