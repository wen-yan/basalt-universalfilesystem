using System;
using System.Threading.Tasks;
using BasaltHexagons.UniversalFileSystem.Core;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class PutObjectTests
{
    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task PutObject_FileInRoot(UniversalFileSystemTestWrapper ufs)
    {
        // test
        await ufs.PutObjectAsync("test.txt", "test content", true);

        // verify
        UniversalFileSystemAssert.VerifyObject(ufs, "test.txt", ObjectType.File, "test content");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task PutObject_FileInSubDirectory(UniversalFileSystemTestWrapper ufs)
    {
        // test
        await ufs.PutObjectAsync("dir/test.txt", "test content", true);

        // verify
        UniversalFileSystemAssert.VerifyObject(ufs, "dir/test.txt", ObjectType.File, "test content");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task PutObject_Overwrite(UniversalFileSystemTestWrapper ufs)
    {
        // test
        await ufs.PutObjectAsync("test.txt", "test content 1", false);

        // verify
        UniversalFileSystemAssert.VerifyObject(ufs, "test.txt", ObjectType.File, "test content 1");

        // test
        await ufs.PutObjectAsync("test.txt", "test content 2", true);

        // verify
        UniversalFileSystemAssert.VerifyObject(ufs, "test.txt", ObjectType.File, "test content 2");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task PutObject_NotOverwrite(UniversalFileSystemTestWrapper ufs)
    {
        // test
        await ufs.PutObjectAsync("test.txt", "test content 1", true);

        // verify
        UniversalFileSystemAssert.VerifyObject(ufs, "test.txt", ObjectType.File, "test content 1");

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

        UniversalFileSystemAssert.VerifyObject(ufs, "test.txt", ObjectType.File, "test content 1");
    }
}