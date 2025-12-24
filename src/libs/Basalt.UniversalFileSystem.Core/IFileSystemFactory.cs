using Microsoft.Extensions.Configuration;

namespace Basalt.UniversalFileSystem.Core;

/// <summary>
/// Interface of IFileSystem factory.
/// </summary>
public interface IFileSystemFactory
{
    /// <summary>
    /// Create IFileSystem from configuration.
    /// </summary>
    /// <param name="configuration">Configuration object.</param>
    /// <returns>Filesystem object.</returns>
    IFileSystem Create(IConfigurationSection configuration);
}