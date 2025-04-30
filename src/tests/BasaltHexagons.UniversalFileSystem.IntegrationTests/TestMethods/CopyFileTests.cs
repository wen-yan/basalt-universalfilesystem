using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.Core.Exceptions;
using BasaltHexagons.UniversalFileSystem.TestUtils;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class CopyFileTests
{
    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetTwoUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task CopyFile_ToSameDirectory(IUniversalFileSystem ufs, UriWrapper u1, UriWrapper u2)
    {
        // setup
        await ufs.PutFileAsync(u1.GetFullUri("test1.txt"), "test content", false);

        // test
        await ufs.CopyFileAsync(u1.GetFullUri("test1.txt"), u2.GetFullUri("test2.txt"), false);

        // verify
        ufs.VerifyObject(u1.GetFullUri("test1.txt"), ObjectType.File, "test content");
        ufs.VerifyObject(u2.GetFullUri("test2.txt"), ObjectType.File, "test content");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetTwoUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task CopyFile_ToDifferentDirectory(IUniversalFileSystem ufs, UriWrapper u1, UriWrapper u2)
    {
        // setup
        await ufs.PutFileAsync(u1.GetFullUri("test1.txt"), "test content", false);

        // test
        await ufs.CopyFileAsync(u1.GetFullUri("test1.txt"), u2.GetFullUri("dir/test2.txt"), false);

        // verify
        ufs.VerifyObject(u1.GetFullUri("test1.txt"), ObjectType.File, "test content");
        ufs.VerifyObject(u2.GetFullUri("dir/test2.txt"), ObjectType.File, "test content");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetTwoUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task CopyFile_Overwrite(IUniversalFileSystem ufs, UriWrapper u1, UriWrapper u2)
    {
        // setup
        await ufs.PutFileAsync(u1.GetFullUri("test1.txt"), "test content 1", false);
        await ufs.PutFileAsync(u2.GetFullUri("test2.txt"), "test content 2", false);

        // test
        await ufs.CopyFileAsync(u1.GetFullUri("test1.txt"), u2.GetFullUri("test2.txt"), true);

        // verify
        ufs.VerifyObject(u1.GetFullUri("test1.txt"), ObjectType.File, "test content 1");
        ufs.VerifyObject(u2.GetFullUri("test2.txt"), ObjectType.File, "test content 1");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetTwoUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task CopyFile_NotOverwrite(IUniversalFileSystem ufs, UriWrapper u1, UriWrapper u2)
    {
        // setup
        await ufs.PutFileAsync(u1.GetFullUri("test1.txt"), "test content 1", false);
        await ufs.PutFileAsync(u2.GetFullUri("test2.txt"), "test content 2", false);

        // test
        await Assert.That.ExpectException<FileExistsException>(async () => await ufs.CopyFileAsync(u1.GetFullUri("test1.txt"), u2.GetFullUri("test2.txt"), false));

        // verify
        ufs.VerifyObject(u1.GetFullUri("test1.txt"), ObjectType.File, "test content 1");
        ufs.VerifyObject(u2.GetFullUri("test2.txt"), ObjectType.File, "test content 2");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task CopyFile_CopyToItself(IUniversalFileSystem ufs, UriWrapper u)
    {
        // setup
        await ufs.PutFileAsync(u.GetFullUri("test1.txt"), "test content 1", false);

        // test
        await Assert.That.ExpectException<ArgumentException>(async () => await ufs.CopyFileAsync(u.GetFullUri("test1.txt"), u.GetFullUri("test1.txt"), true));

        // verify
        ufs.VerifyObject(u.GetFullUri("test1.txt"), ObjectType.File, "test content 1");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetTwoUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task CopyFile_SourceNotExist(IUniversalFileSystem ufs, UriWrapper u1, UriWrapper u2)
    {
        // setup
        await ufs.PutFileAsync(u1.GetFullUri("test1.txt"), "test content 1", false);

        // test
        await Assert.That.ExpectException<FileNotExistsException>(async () => await ufs.CopyFileAsync(u1.GetFullUri("test2.txt"), u2.GetFullUri("test1.txt"), true));

        // verify
        ufs.VerifyObject(u1.GetFullUri("test1.txt"), ObjectType.File, "test content 1");
    }
}