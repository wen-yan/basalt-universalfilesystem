using System.Threading.Tasks;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class DeleteFileTests
{
    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task DeleteFile_FileInRoot(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("test.txt", "test content", true);
        Assert.IsTrue(await ufs.DoesFileExistAsync("test.txt"));

        // test
        bool deleted = await ufs.DeleteFileAsync("test.txt");

        // verify
        Assert.IsTrue(deleted);
        Assert.IsFalse(await ufs.DoesFileExistAsync("test.txt"));
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task DeleteFile_FileInSubDirectory(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("dir/test.txt", "test content", true);
        Assert.IsTrue(await ufs.DoesFileExistAsync("dir/test.txt"));

        // test
        bool deleted = await ufs.DeleteFileAsync("dir/test.txt");

        // verify
        Assert.IsTrue(deleted);
        Assert.IsFalse(await ufs.DoesFileExistAsync("dir/test.txt"));
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task DeleteFile_FileNotExist(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        Assert.IsFalse(await ufs.DoesFileExistAsync("test.txt"));

        // test
        bool deleted = await ufs.DeleteFileAsync("test.txt");

        // verify
        Assert.IsFalse(deleted);
        Assert.IsFalse(await ufs.DoesFileExistAsync("test.txt"));
    }
}