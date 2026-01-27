using Basalt.UniversalFileSystem.Core;
using Basalt.UniversalFileSystem.Core.Exceptions;
using Basalt.UniversalFileSystem.IntegrationTests.Utils;
using Basalt.UniversalFileSystem.TestUtils;

namespace Basalt.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class PutFileTests
{
    [DataTestMethod]
    [SingleUniversalFileSystemTestDataSource]
    public async Task PutFile_FileInRoot(IUniversalFileSystem ufs, UriWrapper u)
    {
        using var _ = await UniversalFileSystemUtils.InitializeFileSystemsAsync(ufs, u);

        // test
        await ufs.PutFileAsync(u.GetFullUri("test.txt"), "test content", true);

        // verify
        ufs.VerifyObject(u.GetFullUri("test.txt"), ObjectType.File, "test content");
    }

    [DataTestMethod]
    [SingleUniversalFileSystemTestDataSource]
    public async Task PutFile_FileInSubDirectory(IUniversalFileSystem ufs, UriWrapper u)
    {
        using var _ = await UniversalFileSystemUtils.InitializeFileSystemsAsync(ufs, u);

        // test
        await ufs.PutFileAsync(u.GetFullUri("dir/test.txt"), "test content", true);

        // verify
        ufs.VerifyObject(u.GetFullUri("dir/test.txt"), ObjectType.File, "test content");
    }

    [DataTestMethod]
    [SingleUniversalFileSystemTestDataSource]
    public async Task PutFile_Overwrite(IUniversalFileSystem ufs, UriWrapper u)
    {
        using var _ = await UniversalFileSystemUtils.InitializeFileSystemsAsync(ufs, u);

        // test
        await ufs.PutFileAsync(u.GetFullUri("test.txt"), "test content 1", false);

        // verify
        ufs.VerifyObject(u.GetFullUri("test.txt"), ObjectType.File, "test content 1");

        // test
        await ufs.PutFileAsync(u.GetFullUri("test.txt"), "test content 2", true);

        // verify
        ufs.VerifyObject(u.GetFullUri("test.txt"), ObjectType.File, "test content 2");
    }

    [DataTestMethod]
    [SingleUniversalFileSystemTestDataSource]
    public async Task PutFile_NotOverwrite(IUniversalFileSystem ufs, UriWrapper u)
    {
        using var _ = await UniversalFileSystemUtils.InitializeFileSystemsAsync(ufs, u);

        // test
        await ufs.PutFileAsync(u.GetFullUri("test.txt"), "test content 1", true);

        // verify
        ufs.VerifyObject(u.GetFullUri("test.txt"), ObjectType.File, "test content 1");

        // test
        await Assert.That.ExpectException<FileExistsException>(async () => await ufs.PutFileAsync(u.GetFullUri("test.txt"), "test content 2", false));

        ufs.VerifyObject(u.GetFullUri("test.txt"), ObjectType.File, "test content 1");
    }
}