using BasaltHexagons.UniversalFileSystem.Core;
using Microsoft.Extensions.Configuration;

namespace BasaltHexagons.UniversalFileSystem.File;

class FileFileSystemFactory : IFileSystemFactory
{
    public IFileSystem Create(IConfiguration implementationConfiguration)
    {
        return new FileFileSystem();
    }
}