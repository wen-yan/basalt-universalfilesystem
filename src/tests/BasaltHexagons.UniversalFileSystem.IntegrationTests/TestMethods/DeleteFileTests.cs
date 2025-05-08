
namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class DeleteFileTests
{
    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task DeleteFile_FileInRoot(IUniversalFileSystem ufs, UriWrapper u)
    {
        // setup
        await ufs.PutFileAsync(u.GetFullUri("test.txt"), "test content", true);
        Assert.IsTrue(await ufs.DoesFileExistAsync(u.GetFullUri("test.txt")));

        // test
        bool deleted = await ufs.DeleteFileAsync(u.GetFullUri("test.txt"));

        // verify
        Assert.IsTrue(deleted);
        Assert.IsFalse(await ufs.DoesFileExistAsync(u.GetFullUri("test.txt")));
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task DeleteFile_FileInSubDirectory(IUniversalFileSystem ufs, UriWrapper u)
    {
        // setup
        await ufs.PutFileAsync(u.GetFullUri("dir/test.txt"), "test content", true);
        Assert.IsTrue(await ufs.DoesFileExistAsync(u.GetFullUri("dir/test.txt")));

        // test
        bool deleted = await ufs.DeleteFileAsync(u.GetFullUri("dir/test.txt"));

        // verify
        Assert.IsTrue(deleted);
        Assert.IsFalse(await ufs.DoesFileExistAsync(u.GetFullUri("dir/test.txt")));
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task DeleteFile_FileNotExist(IUniversalFileSystem ufs, UriWrapper u)
    {
        // setup
        Assert.IsFalse(await ufs.DoesFileExistAsync(u.GetFullUri("test.txt")));

        // test
        bool deleted = await ufs.DeleteFileAsync(u.GetFullUri("test.txt"));

        // verify
        Assert.IsFalse(deleted);
        Assert.IsFalse(await ufs.DoesFileExistAsync(u.GetFullUri("test.txt")));
    }
}