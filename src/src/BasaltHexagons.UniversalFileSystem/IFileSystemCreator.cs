using System;
using BasaltHexagons.UniversalFileSystem.Core;

namespace BasaltHexagons.UniversalFileSystem;

interface IFileSystemCreator
{
    IFileSystem Create(Uri uri);
}