using System.Threading.Tasks;
using BasaltHexagons.UniversalFileSystem.Core;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class GetObjectMetadataTests
{
    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetObjectMetadata_File(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutObjectAsync("test.txt", "test content", true);

        // test
        ObjectMetadata? metadata = await ufs.GetObjectMetadataAsync("test.txt");

        // Verify
        Assert.IsNotNull(metadata);
        Assert.AreEqual(ObjectType.File, metadata.ObjectType);
        Assert.AreEqual("test content".Length, metadata.ContentSize);
        Assert.IsNotNull(metadata.LastModifiedTimeUtc);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetObjectMetadata_Prefix(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutObjectAsync("dir/test.txt", "test content", true);

        // test
        ObjectMetadata? metadata = await ufs.GetObjectMetadataAsync("dir");

        // Verify
        Assert.IsNotNull(metadata);
        Assert.AreEqual(ObjectType.Prefix, metadata.ObjectType);
        Assert.IsNull(metadata.ContentSize);
        Assert.IsNull(metadata.LastModifiedTimeUtc);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetObjectMetadata_NotExists(UniversalFileSystemTestWrapper ufs)
    {
        // test
        ObjectMetadata? metadata = await ufs.GetObjectMetadataAsync("test.txt");

        // verify
        Assert.IsNull(metadata);
    }
}