using System.Threading.Tasks;
using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.TestUtils;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class PutFileTests
{
    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task PutFile_FileInRoot(UniversalFileSystemTestWrapper ufs)
    {
        // test
        await ufs.PutFileAsync("test.txt", "test content", true);

        // verify
        UniversalFileSystemAssert.VerifyObject(ufs, "test.txt", ObjectType.File, "test content");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task PutFile_FileInSubDirectory(UniversalFileSystemTestWrapper ufs)
    {
        // test
        await ufs.PutFileAsync("dir/test.txt", "test content", true);

        // verify
        UniversalFileSystemAssert.VerifyObject(ufs, "dir/test.txt", ObjectType.File, "test content");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task PutFile_Overwrite(UniversalFileSystemTestWrapper ufs)
    {
        // test
        await ufs.PutFileAsync("test.txt", "test content 1", false);

        // verify
        UniversalFileSystemAssert.VerifyObject(ufs, "test.txt", ObjectType.File, "test content 1");

        // test
        await ufs.PutFileAsync("test.txt", "test content 2", true);

        // verify
        UniversalFileSystemAssert.VerifyObject(ufs, "test.txt", ObjectType.File, "test content 2");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task PutFile_NotOverwrite(UniversalFileSystemTestWrapper ufs)
    {
        // test
        await ufs.PutFileAsync("test.txt", "test content 1", true);

        // verify
        UniversalFileSystemAssert.VerifyObject(ufs, "test.txt", ObjectType.File, "test content 1");

        // test
        Assert.That.ExpectException(async () =>  await ufs.PutFileAsync("test.txt", "test content 2", false));

        UniversalFileSystemAssert.VerifyObject(ufs, "test.txt", ObjectType.File, "test content 1");
    }
}