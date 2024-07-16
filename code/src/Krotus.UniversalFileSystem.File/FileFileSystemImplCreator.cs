using Krotus.UniversalFileSystem.Core;
using Microsoft.Extensions.Configuration;

namespace Krotus.UniversalFileSystem.File;

public class FileFileSystemImplCreator : IFileSystemImplCreator
{
    public IFileSystemImpl Create(IConfiguration configuration)
    {
        return new FileFileSystemImpl(configuration);
    }
}