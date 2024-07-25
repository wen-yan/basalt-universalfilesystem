using Krotus.UniversalFileSystem.Core;

namespace Krotus.UniversalFileSystem;

public interface IFileSystemImplFactory
{
    IFileSystem Create(string scheme);
}