using Microsoft.Extensions.Configuration;

namespace Krotus.UniversalFileSystem.Core;

public interface IFileSystemImplCreator
{
    IFileSystemImpl Create(IConfiguration configuration);
}