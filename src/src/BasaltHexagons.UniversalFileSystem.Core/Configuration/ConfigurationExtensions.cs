using System;

using Microsoft.Extensions.Configuration;

namespace BasaltHexagons.UniversalFileSystem.Core.Configuration;

public static class ConfigurationExtensions
{
    public static T? GetValue<T>(this IConfiguration configuration, string key, Func<T?> defaultValueFactory)
    {
        string? value = configuration[key];
        return (T?)Convert.ChangeType(value, typeof(T)) ?? defaultValueFactory();
    }

    public static T GetValue<T>(this IConfiguration configuration, string key)
        => configuration.GetValue<T>(key, () => throw new ConfigurationException($"Configuration key [{key}] is not set."))!;

    public static T? GetEnumValue<T>(this IConfiguration configuration, string key, Func<T?> defaultValueFactory) where T : struct, Enum
    {
        string? valueStr = configuration[key];
        if (valueStr == null)
            return defaultValueFactory();

        if (!Enum.TryParse<T>(valueStr, true, out T value))
        {
            string validValues = string.Join(", ", Enum.GetNames<T>());
            throw new ConfigurationException($"Invalid configuration value [{valueStr}] of key [{key}], valid values are [{validValues}]");
        }
        return value;
    }

    public static T GetEnumValue<T>(this IConfiguration configuration, string key) where T : struct, Enum
    {
        return configuration.GetEnumValue<T>(key, () => null)
            ?? throw new ConfigurationException($"Configuratin key [{key}] is not set.");
    }

    public static bool? GetBoolValue(this IConfiguration configuration, string key, Func<bool?> defaultValueFactory)
    {
        string? valueStr = configuration[key];
        if (valueStr == null)
            return defaultValueFactory();

        return bool.TryParse(valueStr, out bool value)
            ? value
            : throw new ConfigurationException($"Invalid configuration value [{valueStr}] of key [{key}], valid values are [true, false]");
    }
}
