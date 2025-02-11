using BasaltHexagons.UniversalFileSystem.Core;

namespace BasaltHexagons.UniversalFileSystem;

interface IFileSystemCreator
{
    IFileSystem Create(string scheme);
}