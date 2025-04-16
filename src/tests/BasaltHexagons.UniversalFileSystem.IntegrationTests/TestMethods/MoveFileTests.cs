using System;
using System.Threading.Tasks;
using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.Core.Exceptions;
using BasaltHexagons.UniversalFileSystem.TestUtils;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class MoveFileTests
{
    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task MoveFile_ToSameDirectory(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("test.txt", "test content", false);

        // test
        await ufs.MoveFileAsync("test.txt", "test2.txt", false);

        // verify
        Assert.IsFalse(await ufs.DoesFileExistAsync("test.txt"));
        UniversalFileSystemAssert.VerifyObject(ufs, "test2.txt", ObjectType.File, "test content");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task MoveFile_ToDifferentDirectory(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("test.txt", "test content", false);

        // test
        await ufs.MoveFileAsync("test.txt", "dir/test.txt", false);

        // verify
        Assert.IsFalse(await ufs.DoesFileExistAsync("test.txt"));
        UniversalFileSystemAssert.VerifyObject(ufs, "dir/test.txt", ObjectType.File, "test content");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task MoveFile_Overwrite(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("test.txt", "test content", false);
        await ufs.PutFileAsync("test2.txt", "test content2", false);

        // test
        await ufs.MoveFileAsync("test.txt", "test2.txt", true);

        // verify
        Assert.IsFalse(await ufs.DoesFileExistAsync("test.txt"));
        UniversalFileSystemAssert.VerifyObject(ufs, "test2.txt", ObjectType.File, "test content");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task MoveFile_NotOverwrite(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("test.txt", "test content", false);
        await ufs.PutFileAsync("test2.txt", "test content2", false);

        // test
        await Assert.That.ExpectException<FileExistsException>(async () => await ufs.MoveFileAsync("test.txt", "test2.txt", false));

        // verify
        UniversalFileSystemAssert.VerifyObject(ufs, "test.txt", ObjectType.File, "test content");
        UniversalFileSystemAssert.VerifyObject(ufs, "test2.txt", ObjectType.File, "test content2");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task MoveFile_MoveToItself(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("test.txt", "test content", false);

        // test
        await Assert.That.ExpectException<ArgumentException>(async () => await ufs.MoveFileAsync("test.txt", "test.txt", true));

        // verify
        UniversalFileSystemAssert.VerifyObject(ufs, "test.txt", ObjectType.File, "test content");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task MoveFile_SourceNotExist(UniversalFileSystemTestWrapper ufs)
    {
        // test
        await Assert.That.ExpectException<FileNotExistsException>(async () => await ufs.MoveFileAsync("test.txt", "test2.txt", true));
    }
}