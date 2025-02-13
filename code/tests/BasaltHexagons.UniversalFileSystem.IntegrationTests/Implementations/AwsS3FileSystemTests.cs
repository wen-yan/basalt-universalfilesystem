using BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.Implementations;


public interface IAwsS3FileSystemTests : IFileSystemTestsBase
{
}

[TestClass]
public class AwsS3FileSystem_ListObjectsTests : ListObjectsTests, IAwsS3FileSystemTests
{
}

[TestClass]
public class AwsS3FileSystem_GetObjectMetadataTests : GetObjectMetadataTests, IAwsS3FileSystemTests
{
}