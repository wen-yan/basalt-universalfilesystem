using System;

namespace BasaltHexagons.UniversalFileSystem.Core.Configuration;

public class ConfigurationException : Exception
{
    public ConfigurationException(string? message) : base(message)
    {
    }

    public ConfigurationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
