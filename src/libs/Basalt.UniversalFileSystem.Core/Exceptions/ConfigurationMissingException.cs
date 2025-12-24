using System;

namespace Basalt.UniversalFileSystem.Core.Exceptions;

/// <summary>
/// Exception to configuration missing.
/// </summary>
public class ConfigurationMissingException : ConfigurationException
{
    /// <inheritdoc />
    public ConfigurationMissingException(string configurationKey, string? message = null, Exception? inner = null)
        : base(configurationKey, $"Configuration [{configurationKey}] is required.", inner)
    {
    }
}