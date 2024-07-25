using Krotus.UniversalFileSystem.Core;
using Microsoft.Extensions.Configuration;

namespace Krotus.UniversalFileSystem.File;

public class FileFileSystemFactory : IFileSystemFactory
{
    public IFileSystem Create(IConfiguration configuration) => new FileFileSystem();
}