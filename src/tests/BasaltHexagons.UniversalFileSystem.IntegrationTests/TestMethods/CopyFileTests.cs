using System;
using System.Threading.Tasks;
using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.Core.Exceptions;
using BasaltHexagons.UniversalFileSystem.TestUtils;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class CopyFileTests
{
    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task CopyFile_ToSameDirectory(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("test1.txt", "test content", false);

        // test
        await ufs.CopyFileAsync("test1.txt", "test2.txt", false);

        // verify
        UniversalFileSystemAssert.VerifyObject(ufs, "test1.txt", ObjectType.File, "test content");
        UniversalFileSystemAssert.VerifyObject(ufs, "test2.txt", ObjectType.File, "test content");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task CopyFile_ToDifferentDirectory(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("test1.txt", "test content", false);

        // test
        await ufs.CopyFileAsync("test1.txt", "dir/test2.txt", false);

        // verify
        UniversalFileSystemAssert.VerifyObject(ufs, "test1.txt", ObjectType.File, "test content");
        UniversalFileSystemAssert.VerifyObject(ufs, "dir/test2.txt", ObjectType.File, "test content");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task CopyFile_Overwrite(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("test1.txt", "test content 1", false);
        await ufs.PutFileAsync("test2.txt", "test content 2", false);

        // test
        await ufs.CopyFileAsync("test1.txt", "test2.txt", true);

        // verify
        UniversalFileSystemAssert.VerifyObject(ufs, "test1.txt", ObjectType.File, "test content 1");
        UniversalFileSystemAssert.VerifyObject(ufs, "test2.txt", ObjectType.File, "test content 1");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task CopyFile_NotOverwrite(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("test1.txt", "test content 1", false);
        await ufs.PutFileAsync("test2.txt", "test content 2", false);

        // test
        await Assert.That.ExpectException<FileExistsException>(async () => await ufs.CopyFileAsync("test1.txt", "test2.txt", false));

        // verify
        UniversalFileSystemAssert.VerifyObject(ufs, "test1.txt", ObjectType.File, "test content 1");
        UniversalFileSystemAssert.VerifyObject(ufs, "test2.txt", ObjectType.File, "test content 2");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task CopyFile_CopyToItself(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("test1.txt", "test content 1", false);

        // test
        await Assert.That.ExpectException<ArgumentException>(async () => await ufs.CopyFileAsync("test1.txt", "test1.txt", true));

        // verify
        UniversalFileSystemAssert.VerifyObject(ufs, "test1.txt", ObjectType.File, "test content 1");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task CopyFile_SourceNotExist(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("test1.txt", "test content 1", false);

        // test
        await Assert.That.ExpectException<FileNotExistsException>(async () => await ufs.CopyFileAsync("test2.txt", "test1.txt", true));

        // verify
        UniversalFileSystemAssert.VerifyObject(ufs, "test1.txt", ObjectType.File, "test content 1");
    }
}