using System;

namespace Basalt.UniversalFileSystem.Core.Exceptions;

/// <summary>
/// Exception to invalid configurations.
/// </summary>
public class InvalidConfigurationValueException : ConfigurationException
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="configurationKey">Configuration key.</param>
    /// <param name="configurationValue">Configuration value.</param>
    /// <param name="message">Exception message.</param>
    /// <param name="inner">Inner exception.</param>
    public InvalidConfigurationValueException(string configurationKey, string configurationValue, string? message = null, Exception? inner = null)
        : base(configurationKey, message ?? $"Invalid configuration [{configurationKey}] = [{configurationValue}].", inner)
    {
        this.ConfigurationValue = configurationValue;
    }

    /// <summary>
    /// Configuration value.
    /// </summary>
    public string ConfigurationValue { get; }
}