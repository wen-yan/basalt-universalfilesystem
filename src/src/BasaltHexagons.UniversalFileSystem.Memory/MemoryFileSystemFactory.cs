using BasaltHexagons.UniversalFileSystem.Core;
using Microsoft.Extensions.Configuration;

namespace BasaltHexagons.UniversalFileSystem.Memory;

class MemoryFileSystemFactory : IFileSystemFactory
{
    public IFileSystem Create(IConfiguration implementationConfiguration)
    {
        return new MemoryFileSystem();
    }
}