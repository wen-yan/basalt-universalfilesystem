using Microsoft.Extensions.Configuration;

namespace BasaltHexagons.UniversalFileSystem.Core;

public interface IFileSystemFactory
{
    IFileSystem Create(IConfiguration configuration);
}