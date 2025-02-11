using System;

using Microsoft.Extensions.Configuration;

namespace BasaltHexagons.UniversalFileSystem.Core.Configuration;

public static class ConfigurationExtensions
{
    public static T GetValue<T>(this IConfiguration configuration, string key, Func<T> defaultValueFactory)
    {
        string? value = configuration[key];
        return (T?)Convert.ChangeType(value, typeof(T)) ?? defaultValueFactory();
    }
}
