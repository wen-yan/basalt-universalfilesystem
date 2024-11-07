using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;

namespace Krotus.UniversalFileSystem.Cli.Output;

enum TabularDatasetWriterAlignment
{
    Left,
    Center,
    Right
}

class TabularDatasetWriterAttribute : Attribute
{
    public string? ColumnName { get; set; }
    public TabularDatasetWriterAlignment Alignment { get; set; } = TabularDatasetWriterAlignment.Left;
    public string? ToStringMethodName { get; set; }
}

class TabularDatasetWriter : IDatasetWriter
{
    public async ValueTask WriteAsync<T>(IAsyncEnumerable<T> dataset, CancellationToken cancellationToken)
    {
        static string? DefaultToString(object? item, object? value)
        {
            return value?.ToString();
        }

        Type type = typeof(T);
        List<(PropertyInfo Property, TabularDatasetWriterAttribute? Attribute, Func<object?, object?> GetValueFunc, Func<object?, object?, string?> ToStringFunc)> properties = type.GetProperties()
            .Where(x => !x.IsSpecialName)
            .Where(x => x.CanRead)
            .Where(x => (x.GetGetMethod()?.IsPublic ?? false) && (x.GetGetMethod()?.IsStatic ?? true) == false)
            .Select(x =>
            {
                TabularDatasetWriterAttribute? attribute = x.GetCustomAttribute<TabularDatasetWriterAttribute>();
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
        foreach ((PropertyInfo Property, TabularDatasetWriterAttribute? Attribute, Func<object?, object?> GetValueFunc, Func<object?, object?, string?> ToStringFunc) property in properties)
        {
            string columnName = property.Attribute?.ColumnName ?? property.Property.Name;
            TabularDatasetWriterAlignment alignment = property.Attribute?.Alignment ?? TabularDatasetWriterAlignment.Left;

            TableColumn column = new(columnName)
            {
                Alignment = alignment switch
                {
                    TabularDatasetWriterAlignment.Left => Justify.Left,
                    TabularDatasetWriterAlignment.Right => Justify.Right,
                    TabularDatasetWriterAlignment.Center => Justify.Center,
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