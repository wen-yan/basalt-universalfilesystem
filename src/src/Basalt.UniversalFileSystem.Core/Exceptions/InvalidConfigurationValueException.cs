using System;

namespace Basalt.UniversalFileSystem.Core.Exceptions;

public class InvalidConfigurationValueException : ConfigurationException
{
    public InvalidConfigurationValueException(string configurationKey, string configurationValue, string? message = null, Exception? inner = null)
        : base(configurationKey, message ?? $"Invalid configuration [{configurationKey}] = [{configurationValue}].", inner)
    {
        this.ConfigurationValue = configurationValue;
    }

    public string ConfigurationValue { get; }
}