using System;

namespace BasaltHexagons.UniversalFileSystem.Core.Exceptions;

public class ConfigurationMissingException : ConfigurationException
{
    public ConfigurationMissingException(string configurationKey, string? message = null, Exception? inner = null)
        : base(configurationKey, $"Configuration [{configurationKey}] is required.", inner)
    {
    }
}