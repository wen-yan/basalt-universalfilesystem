using Microsoft.Extensions.Configuration;

namespace Basalt.UniversalFileSystem.Core;

public interface IFileSystemFactory
{
    IFileSystem Create(IConfigurationSection configuration);
}