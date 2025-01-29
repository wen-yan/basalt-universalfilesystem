using BasaltHexagons.UniversalFileSystem.Core;

namespace BasaltHexagons.UniversalFileSystem;

public interface IFileSystemImplFactory
{
    IFileSystem Create(string scheme);
}