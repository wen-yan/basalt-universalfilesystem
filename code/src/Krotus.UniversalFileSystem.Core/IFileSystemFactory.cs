using Microsoft.Extensions.Configuration;

namespace Krotus.UniversalFileSystem.Core;

public interface IFileSystemFactory
{
    IFileSystem Create(IConfiguration configuration);
}