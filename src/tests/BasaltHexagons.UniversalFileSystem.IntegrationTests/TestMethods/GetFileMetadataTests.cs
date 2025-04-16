using System;
using System.Threading.Tasks;
using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.Core.Exceptions;
using BasaltHexagons.UniversalFileSystem.TestUtils;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class GetFileMetadataTests
{
    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetFileMetadata_File(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("test.txt", "test content", true);

        // test
        ObjectMetadata? metadata = await ufs.GetFileMetadataAsync("test.txt");

        // Verify
        UniversalFileSystemAssert.VerifyObject(ufs, "test.txt", ObjectType.File, "test content", metadata);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetFileMetadata_Prefix(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("dir/test.txt", "test content", true);

        // test
        await Assert.That.ExpectException<FileNotExistsException>(async () => await ufs.GetFileMetadataAsync("dir"));
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetFileMetadata_NotExists(UniversalFileSystemTestWrapper ufs)
    {
        // test
        await Assert.That.ExpectException<FileNotExistsException>(async () => await ufs.GetFileMetadataAsync("test.txt"));
    }
}