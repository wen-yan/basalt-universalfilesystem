using System.Runtime.CompilerServices;
using Basalt.UniversalFileSystem.Core;
using Microsoft.Extensions.Configuration;

namespace Basalt.UniversalFileSystem.File;

[FileSystemFactoryConfigurationTemplate(
    """
    File:
      UriRegexPattern: ^file:///.*$
      FileSystemFactoryClass: Basalt.UniversalFileSystem.File.FileFileSystemFactory
    """)]
[AsyncMethodBuilder(typeof(ContinueOnAnyAsyncMethodBuilder))]
class FileFileSystemFactory : IFileSystemFactory
{
    public IFileSystem Create(IConfigurationSection configuration)
    {
        return new FileFileSystem();
    }
}