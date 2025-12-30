using System.Runtime.CompilerServices;
using Basalt.UniversalFileSystem.Core;
using Microsoft.Extensions.Configuration;

namespace Basalt.UniversalFileSystem.Memory;

[FileSystemFactoryConfigurationTemplate(
    """
    Memory:
      UriRegexPattern: ^memory:///.*$
      FileSystemFactoryClass: Basalt.UniversalFileSystem.Memory.MemoryFileSystemFactory
    """)]
class MemoryFileSystemFactory : IFileSystemFactory
{
    public IFileSystem Create(IConfigurationSection configuration)
    {
        return new MemoryFileSystem();
    }
}