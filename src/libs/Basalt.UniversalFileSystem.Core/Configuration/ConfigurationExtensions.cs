using System;
using Basalt.UniversalFileSystem.Core.Exceptions;
using Microsoft.Extensions.Configuration;

namespace Basalt.UniversalFileSystem.Core.Configuration;

/// <summary>
/// Configuration extension methods
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Get value from configuration by key, supporting default value factory.
    /// </summary>
    /// <param name="configuration">IConfiguration object.</param>
    /// <param name="key">Key of value.</param>
    /// <param name="defaultValueFactory">Default value factory when key is missing.</param>
    /// <typeparam name="T">Template type of value.</typeparam>
    /// <returns>Nullable value of key.</returns>
    public static T? GetValue<T>(this IConfiguration configuration, string key, Func<T?> defaultValueFactory)
    {
        string? value = configuration[key];
        return (T?)Convert.ChangeType(value, typeof(T)) ?? defaultValueFactory();
    }

    /// <summary>
    /// Get value from configuration by key.
    /// </summary>
    /// <param name="configuration">IConfiguration object.</param>
    /// <param name="key">Key of value.</param>
    /// <typeparam name="T">Template type of value.</typeparam>
    /// <returns>Value of key.</returns>
    /// <exception cref="ConfigurationMissingException">Throw exceptions when key is missing.</exception>
    public static T GetValue<T>(this IConfiguration configuration, string key)
        => configuration.GetValue<T>(key, () => throw new ConfigurationMissingException(key))!;

    /// <summary>
    /// Get enum value from configuration by key, supporting default value factory.
    /// </summary>
    /// <param name="configuration">IConfiguration object.</param>
    /// <param name="key">Key of value.</param>
    /// <param name="defaultValueFactory">Default value factory when key is missing.</param>
    /// <typeparam name="T">Template type of value.</typeparam>
    /// <exception cref="InvalidEnumConfigurationValueException{T}">Throw exceptions when value cannot be converted to enum.</exception>
    public static T? GetEnumValue<T>(this IConfiguration configuration, string key, Func<T?> defaultValueFactory) where T : struct, Enum
    {
        string? valueStr = configuration[key];
        if (valueStr == null)
            return defaultValueFactory();

        if (!Enum.TryParse<T>(valueStr, true, out T value))
            throw new InvalidEnumConfigurationValueException<T>(key, valueStr);

        return value;
    }

    /// <summary>
    /// Get enum value from configuration by key.
    /// </summary>
    /// <param name="configuration">IConfiguration object.</param>
    /// <param name="key">Key of value.</param>
    /// <typeparam name="T">Template type of value.</typeparam>
    /// <returns>Value of key.</returns>
    /// <exception cref="ConfigurationMissingException">Throw exceptions when key is missing.</exception>
    public static T GetEnumValue<T>(this IConfiguration configuration, string key) where T : struct, Enum
    {
        return configuration.GetEnumValue<T>(key, () => null) ?? throw new ConfigurationMissingException(key);
    }

    /// <summary>
    /// Get value from configuration by key, supporting default value factory.
    /// </summary>
    /// <param name="configuration">IConfiguration object.</param>
    /// <param name="key">Key of value.</param>
    /// <param name="defaultValueFactory">Default value factory when key is missing.</param>
    /// <returns>Value of key.</returns>
    /// <exception cref="InvalidConfigurationValueException">Throw exceptions when value cannot be converted to boolean.</exception>
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