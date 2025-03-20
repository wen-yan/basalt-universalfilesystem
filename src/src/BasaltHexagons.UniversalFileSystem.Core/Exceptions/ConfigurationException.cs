using System;

namespace BasaltHexagons.UniversalFileSystem.Core.Exceptions;

public abstract class ConfigurationException : UniversalFileSystemException
{
    protected ConfigurationException(string configurationKey, string? message = null, Exception? inner = null)
        : base(message, inner)
    {
        this.ConfigurationKey = configurationKey;
    }

    public string ConfigurationKey { get; }
}