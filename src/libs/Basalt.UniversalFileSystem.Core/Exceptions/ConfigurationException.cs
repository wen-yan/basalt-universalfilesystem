using System;

namespace Basalt.UniversalFileSystem.Core.Exceptions;

/// <summary>
/// Base exception class of all configuration exceptions.
/// </summary>
public abstract class ConfigurationException : UniversalFileSystemException
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="configurationKey">Configuration key.</param>
    /// <param name="message">Exception message.</param>
    /// <param name="inner">Inner exception.</param>
    protected ConfigurationException(string configurationKey, string? message = null, Exception? inner = null)
        : base(message, inner)
    {
        this.ConfigurationKey = configurationKey;
    }

    /// <summary>
    /// Configuration key.
    /// </summary>
    public string ConfigurationKey { get; }
}