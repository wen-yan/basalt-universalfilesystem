using Basalt.UniversalFileSystem.Core;
using Basalt.UniversalFileSystem.Core.Exceptions;
using Basalt.UniversalFileSystem.IntegrationTests.Utils;
using Basalt.UniversalFileSystem.TestUtils;

namespace Basalt.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class MoveFileTests
{
    [DataTestMethod]
    [DoubleUniversalFileSystemTestDataSource]
    public async Task MoveFile_ToSameDirectory(IUniversalFileSystem ufs, UriWrapper u1, UriWrapper u2)
    {
        using var _ = await UniversalFileSystemUtils.InitializeFileSystemsAsync(ufs, u1, u2);

        // setup
        await ufs.PutFileAsync(u1.GetFullUri("test.txt"), "test content", false);

        // test
        await ufs.MoveFileAsync(u1.GetFullUri("test.txt"), u2.GetFullUri("test2.txt"), false);

        // verify
        Assert.IsFalse(await ufs.DoesFileExistAsync(u1.GetFullUri("test.txt")));
        ufs.VerifyObject(u2.GetFullUri("test2.txt"), ObjectType.File, "test content");
    }

    [DataTestMethod]
    [DoubleUniversalFileSystemTestDataSource]
    public async Task MoveFile_ToDifferentDirectory(IUniversalFileSystem ufs, UriWrapper u1, UriWrapper u2)
    {
        using var _ = await UniversalFileSystemUtils.InitializeFileSystemsAsync(ufs, u1, u2);

        // setup
        await ufs.PutFileAsync(u1.GetFullUri("test.txt"), "test content", false);

        // test
        await ufs.MoveFileAsync(u1.GetFullUri("test.txt"), u2.GetFullUri("dir/test.txt"), false);

        // verify
        Assert.IsFalse(await ufs.DoesFileExistAsync(u1.GetFullUri("test.txt")));
        ufs.VerifyObject(u2.GetFullUri("dir/test.txt"), ObjectType.File, "test content");
    }

    [DataTestMethod]
    [DoubleUniversalFileSystemTestDataSource]
    public async Task MoveFile_Overwrite(IUniversalFileSystem ufs, UriWrapper u1, UriWrapper u2)
    {
        using var _ = await UniversalFileSystemUtils.InitializeFileSystemsAsync(ufs, u1, u2);

        // setup
        await ufs.PutFileAsync(u1.GetFullUri("test.txt"), "test content", false);
        await ufs.PutFileAsync(u2.GetFullUri("test2.txt"), "test content2", false);

        // test
        await ufs.MoveFileAsync(u1.GetFullUri("test.txt"), u2.GetFullUri("test2.txt"), true);

        // verify
        Assert.IsFalse(await ufs.DoesFileExistAsync(u1.GetFullUri("test.txt")));
        ufs.VerifyObject(u2.GetFullUri("test2.txt"), ObjectType.File, "test content");
    }

    [DataTestMethod]
    [DoubleUniversalFileSystemTestDataSource]
    public async Task MoveFile_NotOverwrite(IUniversalFileSystem ufs, UriWrapper u1, UriWrapper u2)
    {
        using var _ = await UniversalFileSystemUtils.InitializeFileSystemsAsync(ufs, u1, u2);

        // setup
        await ufs.PutFileAsync(u1.GetFullUri("test.txt"), "test content", false);
        await ufs.PutFileAsync(u2.GetFullUri("test2.txt"), "test content2", false);

        // test
        await Assert.That.ExpectException<FileExistsException>(async () => await ufs.MoveFileAsync(u1.GetFullUri("test.txt"), u2.GetFullUri("test2.txt"), false));

        // verify
        ufs.VerifyObject(u1.GetFullUri("test.txt"), ObjectType.File, "test content");
        ufs.VerifyObject(u2.GetFullUri("test2.txt"), ObjectType.File, "test content2");
    }

    [DataTestMethod]
    [SingleUniversalFileSystemTestDataSource]
    public async Task MoveFile_MoveToItself(IUniversalFileSystem ufs, UriWrapper u)
    {
        using var _ = await UniversalFileSystemUtils.InitializeFileSystemsAsync(ufs, u);

        // setup
        await ufs.PutFileAsync(u.GetFullUri("test.txt"), "test content", false);

        // test
        await Assert.That.ExpectException<ArgumentException>(async () => await ufs.MoveFileAsync(u.GetFullUri("test.txt"), u.GetFullUri("test.txt"), true));

        // verify
        ufs.VerifyObject(u.GetFullUri("test.txt"), ObjectType.File, "test content");
    }

    [DataTestMethod]
    [DoubleUniversalFileSystemTestDataSource]
    public async Task MoveFile_SourceNotExist(IUniversalFileSystem ufs, UriWrapper u1, UriWrapper u2)
    {
        using var _ = await UniversalFileSystemUtils.InitializeFileSystemsAsync(ufs, u1, u2);

        // test
        await Assert.That.ExpectException<FileNotExistsException>(async () => await ufs.MoveFileAsync(u1.GetFullUri("test.txt"), u2.GetFullUri("test2.txt"), true));
    }
}