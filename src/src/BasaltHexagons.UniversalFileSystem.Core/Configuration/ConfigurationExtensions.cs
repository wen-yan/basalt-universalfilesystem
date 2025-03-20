using System;
using BasaltHexagons.UniversalFileSystem.Core.Exceptions;
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
        => configuration.GetValue<T>(key, () => throw new ConfigurationMissingException(key))!;

    public static T? GetEnumValue<T>(this IConfiguration configuration, string key, Func<T?> defaultValueFactory) where T : struct, Enum
    {
        string? valueStr = configuration[key];
        if (valueStr == null)
            return defaultValueFactory();

        if (!Enum.TryParse<T>(valueStr, true, out T value))
            throw new InvalidEnumConfigurationValueException<T>(key, valueStr);

        return value;
    }

    public static T GetEnumValue<T>(this IConfiguration configuration, string key) where T : struct, Enum
    {
        return configuration.GetEnumValue<T>(key, () => null) ?? throw new ConfigurationMissingException(key);
    }

    public static bool? GetBoolValue(this IConfiguration configuration, string key, Func<bool?> defaultValueFactory)
    {
        string? valueStr = configuration[key];
        if (valueStr == null)
            return defaultValueFactory();

        return bool.TryParse(valueStr, out bool value)
            ? value
            : throw new InvalidConfigurationValueException(key, valueStr);
    }
}