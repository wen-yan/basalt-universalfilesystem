using BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.Implementations;

public interface IFileFileSystemTests : IFileSystemTestsBase
{
}

[TestClass]
public class FileFileSystem_ListObjectsTests : ListObjectsTests, IFileFileSystemTests
{
}

[TestClass]
public class FileFileSystem_GetObjectMetadataTests : GetObjectMetadataTests, IFileFileSystemTests
{
}
