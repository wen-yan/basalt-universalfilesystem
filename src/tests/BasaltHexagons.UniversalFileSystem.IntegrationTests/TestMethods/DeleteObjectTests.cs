using System.Threading.Tasks;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class DeleteObjectTests
{
    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task DeleteObject_FileInRoot(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutObjectAsync("test.txt", "test content", true);
        Assert.IsTrue(await ufs.ExistsAsync("test.txt"));

        // test
        bool deleted = await ufs.DeleteObjectAsync("test.txt");

        // verify
        Assert.IsTrue(deleted);
        Assert.IsFalse(await ufs.ExistsAsync("test.txt"));
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task DeleteObject_FileInSubDirectory(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutObjectAsync("dir/test.txt", "test content", true);
        Assert.IsTrue(await ufs.ExistsAsync("dir/test.txt"));

        // test
        bool deleted = await ufs.DeleteObjectAsync("dir/test.txt");

        // verify
        Assert.IsTrue(deleted);
        Assert.IsFalse(await ufs.ExistsAsync("dir/test.txt"));
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task DeleteObject_FileNotExist(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        Assert.IsFalse(await ufs.ExistsAsync("test.txt"));

        // test
        bool deleted = await ufs.DeleteObjectAsync("test.txt");

        // verify
        Assert.IsFalse(deleted);
        Assert.IsFalse(await ufs.ExistsAsync("test.txt"));
    }
}