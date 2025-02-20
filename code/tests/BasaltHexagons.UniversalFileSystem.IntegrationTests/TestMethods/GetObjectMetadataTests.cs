using System;
using System.Threading.Tasks;
using BasaltHexagons.UniversalFileSystem.Core;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class GetObjectMetadataTests
{
    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetObjectMetadata_FileTest(MethodTestsUniversalFileSystemWrapper ufs)
    {
        // setup
        await ufs.PutObjectAsync("test.txt", "test content", true);

        // test
        ObjectMetadata metadata = await ufs.GetObjectMetadataAsync("test.txt");

        // Verify
        Assert.IsNotNull(metadata);
        Assert.AreEqual(ObjectType.File, metadata.ObjectType);
        Assert.AreEqual("test content".Length, metadata.ContentSize);
        Assert.IsNotNull(metadata.LastModifiedTimeUtc);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetObjectMetadata_PrefixTest(MethodTestsUniversalFileSystemWrapper ufs)
    {
        // setup
        await ufs.PutObjectAsync("dir/test.txt", "test content", true);

        // test
        ObjectMetadata metadata = await ufs.GetObjectMetadataAsync("dir");

        // Verify
        Assert.IsNotNull(metadata);
        Assert.AreEqual(ObjectType.Prefix, metadata.ObjectType);
        Assert.IsNull(metadata.ContentSize);
        Assert.IsNull(metadata.LastModifiedTimeUtc);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetObjectMetadata_NotExistsTest(MethodTestsUniversalFileSystemWrapper ufs)
    {
        // test
        try
        {
            ObjectMetadata metadata = await ufs.GetObjectMetadataAsync("test.txt");
            Assert.Fail("Expected exception is not thrown.");
        }
        catch (ArgumentException ex)
        {
            Assert.IsTrue(ex.Message.StartsWith("The path does not exist"));
        }
        catch (Exception)
        {
            Assert.Fail("Expected exception is not thrown.");
        }
    }
}