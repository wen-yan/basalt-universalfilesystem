using System;
using System.Threading.Tasks;
using BasaltHexagons.UniversalFileSystem.Core;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class PutObjectTests
{
    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task PutObject_FileInRootTest(MethodTestsUniversalFileSystemWrapper ufs)
    {
        // test
        await ufs.PutObjectAsync("test.txt", "test content", true);

        // verify
        ObjectMetadata metadata = await ufs.GetObjectMetadataAsync("test.txt");
        Assert.IsNotNull(metadata);
        Assert.AreEqual(ObjectType.File, metadata.ObjectType);
        Assert.AreEqual("test content".Length, metadata.ContentSize);
        Assert.IsNotNull(metadata.LastModifiedTimeUtc);

        string content = await ufs.GetObjectAsync("test.txt");
        Assert.AreEqual("test content", content);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task PutObject_FileInSubDirectoryTest(MethodTestsUniversalFileSystemWrapper ufs)
    {
        // test
        await ufs.PutObjectAsync("dir/test.txt", "test content", true);

        // verify
        ObjectMetadata metadataFile = await ufs.GetObjectMetadataAsync("dir/test.txt");
        Assert.IsNotNull(metadataFile);
        Assert.AreEqual(ObjectType.File, metadataFile.ObjectType);
        Assert.AreEqual("test content".Length, metadataFile.ContentSize);
        Assert.IsNotNull(metadataFile.LastModifiedTimeUtc);

        string content = await ufs.GetObjectAsync("dir/test.txt");
        Assert.AreEqual("test content", content);

        ObjectMetadata metadataDir = await ufs.GetObjectMetadataAsync("dir");
        Assert.IsNotNull(metadataDir);
        Assert.AreEqual(ObjectType.Prefix, metadataDir.ObjectType);
        Assert.IsNull(metadataDir.ContentSize);
        Assert.IsNull(metadataDir.LastModifiedTimeUtc);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task PutObject_OverwriteTest(MethodTestsUniversalFileSystemWrapper ufs)
    {
        // test
        await ufs.PutObjectAsync("test.txt", "test content 1", false);

        // verify
        ObjectMetadata metadata1 = await ufs.GetObjectMetadataAsync("test.txt");
        Assert.IsNotNull(metadata1);
        Assert.AreEqual(ObjectType.File, metadata1.ObjectType);
        Assert.AreEqual("test content 1".Length, metadata1.ContentSize);
        Assert.IsNotNull(metadata1.LastModifiedTimeUtc);

        string content1 = await ufs.GetObjectAsync("test.txt");
        Assert.AreEqual("test content 1", content1);

        // test
        await ufs.PutObjectAsync("test.txt", "test content 2", true);

        // verify
        ObjectMetadata metadata2 = await ufs.GetObjectMetadataAsync("test.txt");
        Assert.IsNotNull(metadata2);
        Assert.AreEqual(ObjectType.File, metadata2.ObjectType);
        Assert.AreEqual("test content 2".Length, metadata2.ContentSize);
        Assert.IsNotNull(metadata2.LastModifiedTimeUtc);

        string content2 = await ufs.GetObjectAsync("test.txt");
        Assert.AreEqual("test content 2", content2);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task PutObject_NotOverwriteTest(MethodTestsUniversalFileSystemWrapper ufs)
    {
        // test
        await ufs.PutObjectAsync("test.txt", "test content 1", true);

        // verify
        ObjectMetadata metadata1 = await ufs.GetObjectMetadataAsync("test.txt");
        Assert.IsNotNull(metadata1);
        Assert.AreEqual(ObjectType.File, metadata1.ObjectType);
        Assert.AreEqual("test content 1".Length, metadata1.ContentSize);
        Assert.IsNotNull(metadata1.LastModifiedTimeUtc);

        string content1 = await ufs.GetObjectAsync("test.txt");
        Assert.AreEqual("test content 1", content1);

        // test
        try
        {
            await ufs.PutObjectAsync("test.txt", "test content 2", false);
            Assert.Fail("Expected exception is not thrown");
        }
        catch (Exception)
        {
            // ignored
        }
    }
}