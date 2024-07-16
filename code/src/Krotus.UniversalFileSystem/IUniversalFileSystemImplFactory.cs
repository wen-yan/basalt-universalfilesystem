using Krotus.UniversalFileSystem.Core;

namespace Krotus.UniversalFileSystem;

public interface IUniversalFileSystemImplFactory
{
    IFileSystemImpl Create(string scheme);
}