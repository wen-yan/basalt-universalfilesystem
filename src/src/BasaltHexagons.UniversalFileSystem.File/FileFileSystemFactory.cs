using System.Runtime.CompilerServices;
using BasaltHexagons.UniversalFileSystem.Core;
using Microsoft.Extensions.Configuration;

namespace BasaltHexagons.UniversalFileSystem.File;

[AsyncMethodBuilder(typeof(ContinueOnAnyAsyncMethodBuilder))]
class FileFileSystemFactory : IFileSystemFactory
{
    public IFileSystem Create(IConfigurationSection configuration)
    {
        return new FileFileSystem();
    }
}