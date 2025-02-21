using System;
using System.Threading.Tasks;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class GetObjectTests
{
    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetObject_FileInRoot(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutObjectAsync("test.txt", "test content", true);

        // test
        string content = await ufs.GetObjectAsync("test.txt");

        // verify
        Assert.AreEqual("test content", content);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetObject_FileInSubDirectory(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutObjectAsync("dir/test.txt", "test content", true);

        // test
        string content = await ufs.GetObjectAsync("dir/test.txt");

        // verify
        Assert.AreEqual("test content", content);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetObject_FileNotExists(UniversalFileSystemTestWrapper ufs)
    {
        // test
        try
        {
            string content = await ufs.GetObjectAsync("test.txt");
            Assert.Fail("Expected exception is not thrown.");
        }
        catch (Exception)
        {
        }
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetObject_SameNameAsDirectory(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutObjectAsync("dir/test.txt", "test content", true);

        // test
        try
        {
            string content = await ufs.GetObjectAsync("dir");
            Assert.Fail("Expected exception is not thrown.");
        }
        catch (Exception)
        {
        }
    }
}