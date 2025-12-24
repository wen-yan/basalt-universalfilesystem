using System;

namespace Basalt.UniversalFileSystem.Core.Exceptions;

/// <summary>
/// Exception to invalid enum configuration.
/// </summary>
/// <typeparam name="T"></typeparam>
public class InvalidEnumConfigurationValueException<T> : InvalidConfigurationValueException where T : struct, Enum
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="configurationKey">Configuration key.</param>
    /// <param name="configurationValue">Configuration value.</param>
    /// <param name="message">Exception message.</param>
    /// <param name="inner">Inner exception.</param>
    /// <exception cref="ArgumentNullException">Argument configurationValue is null.</exception>
    public InvalidEnumConfigurationValueException(string configurationKey, object? configurationValue, string? message = null, Exception? inner = null)
        : base(configurationKey, configurationValue?.ToString() ?? "<null>",
            message ?? $"Invalid configuration [{configurationKey}] = [{configurationValue}], valid values are [{string.Join("|", Enum.GetNames<T>())}]",
            inner)
    {
        if (configurationValue == null) throw new ArgumentNullException(nameof(configurationValue));
    }
}