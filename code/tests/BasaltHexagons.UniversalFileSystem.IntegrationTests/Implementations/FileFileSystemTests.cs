using System;
using BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.Implementations;

public interface IFileFileSystemTests : IFileSystemMethodTests
{
    IUniversalFileSystem IFileSystemMethodTests.GetUniversalFileSystem()
    {
        Console.WriteLine("IFileFileSystemTests.GetUniversalFileSystem()");
        return null!; //throw new System.NotImplementedException();
    }
}

[TestClass]
public class FileFileSystem_ListObjectsTests : ListObjectsTests, IFileFileSystemTests
{
}

[TestClass]
public class FileFileSystem_GetObjectMetadataTests : GetObjectMetadataTests, IFileFileSystemTests
{
}
