using Basalt.UniversalFileSystem.IntegrationTests.Utils;

namespace Basalt.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class DeleteFileTests
{
    [DataTestMethod]
    [SingleUniversalFileSystemTestDataSource]
    public async Task DeleteFile_FileInRoot(IUniversalFileSystem ufs, UriWrapper u)
    {
        using var _ = await UniversalFileSystemUtils.InitializeFileSystemsAsync(ufs, u);

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
    [SingleUniversalFileSystemTestDataSource]
    public async Task DeleteFile_FileInSubDirectory(IUniversalFileSystem ufs, UriWrapper u)
    {
        using var _ = await UniversalFileSystemUtils.InitializeFileSystemsAsync(ufs, u);

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
    [SingleUniversalFileSystemTestDataSource]
    public async Task DeleteFile_FileNotExist(IUniversalFileSystem ufs, UriWrapper u)
    {
        using var _ = await UniversalFileSystemUtils.InitializeFileSystemsAsync(ufs, u);

        // setup
        Assert.IsFalse(await ufs.DoesFileExistAsync(u.GetFullUri("test.txt")));

        // test
        bool deleted = await ufs.DeleteFileAsync(u.GetFullUri("test.txt"));

        // verify
        Assert.IsFalse(deleted);
        Assert.IsFalse(await ufs.DoesFileExistAsync(u.GetFullUri("test.txt")));
    }
}