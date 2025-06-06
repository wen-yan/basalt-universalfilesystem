using Basalt.UniversalFileSystem.Core;
using Basalt.UniversalFileSystem.Core.Exceptions;
using Basalt.UniversalFileSystem.TestUtils;

namespace Basalt.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class GetFileMetadataTests
{
    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetFileMetadata_File(IUniversalFileSystem ufs, UriWrapper u)
    {
        // setup
        await ufs.PutFileAsync(u.GetFullUri("test.txt"), "test content", true);

        // test
        ObjectMetadata? metadata = await ufs.GetFileMetadataAsync(u.GetFullUri("test.txt"));

        // Verify
        ufs.VerifyObject(u.GetFullUri("test.txt"), ObjectType.File, "test content", metadata);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetFileMetadata_Prefix(IUniversalFileSystem ufs, UriWrapper u)
    {
        // setup
        await ufs.PutFileAsync(u.GetFullUri("dir/test.txt"), "test content", true);

        // test
        await Assert.That.ExpectException<FileNotExistsException>(async () => await ufs.GetFileMetadataAsync(u.GetFullUri("dir")));
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetFileMetadata_NotExists(IUniversalFileSystem ufs, UriWrapper u)
    {
        // test
        await Assert.That.ExpectException<FileNotExistsException>(async () => await ufs.GetFileMetadataAsync(u.GetFullUri("test.txt")));
    }
}