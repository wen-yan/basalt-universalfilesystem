using System.Runtime.CompilerServices;
using BasaltHexagons.UniversalFileSystem.Core;
using Microsoft.Extensions.Configuration;

namespace BasaltHexagons.UniversalFileSystem.Memory;

[AsyncMethodBuilder(typeof(ContinueOnAnyAsyncMethodBuilder))]
class MemoryFileSystemFactory : IFileSystemFactory
{
    public IFileSystem Create(IConfigurationSection configuration)
    {
        return new MemoryFileSystem();
    }
}