using Basalt.UniversalFileSystem.Core.Exceptions;
using Basalt.UniversalFileSystem.IntegrationTests.Utils;
using Basalt.UniversalFileSystem.TestUtils;

namespace Basalt.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class GetFileTests
{
    [DataTestMethod]
    [SingleUniversalFileSystemTestDataSource]
    public async Task GetFile_FileInRoot(IUniversalFileSystem ufs, UriWrapper u)
    {
        using var _ = await UniversalFileSystemUtils.InitializeFileSystemsAsync(ufs, u);

        // setup
        await ufs.PutFileAsync(u.GetFullUri("test.txt"), "test content", true);

        // test
        string content = await ufs.GetFileStringAsync(u.GetFullUri("test.txt"));

        // verify
        Assert.AreEqual("test content", content);
    }

    [DataTestMethod]
    [SingleUniversalFileSystemTestDataSource]
    public async Task GetFile_FileInSubDirectory(IUniversalFileSystem ufs, UriWrapper u)
    {
        using var _ = await UniversalFileSystemUtils.InitializeFileSystemsAsync(ufs, u);

        // setup
        await ufs.PutFileAsync(u.GetFullUri("dir/test.txt"), "test content", true);

        // test
        string content = await ufs.GetFileStringAsync(u.GetFullUri("dir/test.txt"));

        // verify
        Assert.AreEqual("test content", content);
    }

    [DataTestMethod]
    [SingleUniversalFileSystemTestDataSource]
    public async Task GetFile_FileNotExists(IUniversalFileSystem ufs, UriWrapper u)
    {
        using var _ = await UniversalFileSystemUtils.InitializeFileSystemsAsync(ufs, u);

        // test
        await Assert.That.ExpectException<FileNotExistsException>(async () => await ufs.GetFileAsync(u.GetFullUri("test.txt")));
        await Task.CompletedTask;
    }

    [DataTestMethod]
    [SingleUniversalFileSystemTestDataSource]
    public async Task GetFile_SameNameAsDirectory(IUniversalFileSystem ufs, UriWrapper u)
    {
        using var _ = await UniversalFileSystemUtils.InitializeFileSystemsAsync(ufs, u);

        // setup
        await ufs.PutFileAsync(u.GetFullUri("dir/test.txt"), "test content", true);

        // test
        await Assert.That.ExpectException<FileNotExistsException>(async () => await ufs.GetFileAsync(u.GetFullUri("dir")));
    }
}