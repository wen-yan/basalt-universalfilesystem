using BasaltHexagons.UniversalFileSystem.Core;
using Microsoft.Extensions.Configuration;

namespace BasaltHexagons.UniversalFileSystem.File;

public class FileFileSystemFactory : IFileSystemFactory
{
    public IFileSystem Create(IConfiguration configuration)
    {
        return new FileFileSystem();
    }
}