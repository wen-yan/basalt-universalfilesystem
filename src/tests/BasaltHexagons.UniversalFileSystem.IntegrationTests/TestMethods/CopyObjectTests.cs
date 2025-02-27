using System.Threading.Tasks;
using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.TestUtils;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class CopyObjectTests
{
    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task CopyObject_ToSameDirectory(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutObjectAsync("test1.txt", "test content", false);

        // test
        await ufs.CopyObjectAsync("test1.txt", "test2.txt", false);

        // verify
        UniversalFileSystemAssert.VerifyObject(ufs, "test1.txt", ObjectType.File, "test content");
        UniversalFileSystemAssert.VerifyObject(ufs, "test2.txt", ObjectType.File, "test content");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task CopyObject_ToDifferentDirectory(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutObjectAsync("test1.txt", "test content", false);

        // test
        await ufs.CopyObjectAsync("test1.txt", "dir/test2.txt", false);

        // verify
        UniversalFileSystemAssert.VerifyObject(ufs, "test1.txt", ObjectType.File, "test content");
        UniversalFileSystemAssert.VerifyObject(ufs, "dir/test2.txt", ObjectType.File, "test content");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task CopyObject_Overwrite(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutObjectAsync("test1.txt", "test content 1", false);
        await ufs.PutObjectAsync("test2.txt", "test content 2", false);

        // test
        await ufs.CopyObjectAsync("test1.txt", "test2.txt", true);

        // verify
        UniversalFileSystemAssert.VerifyObject(ufs, "test1.txt", ObjectType.File, "test content 1");
        UniversalFileSystemAssert.VerifyObject(ufs, "test2.txt", ObjectType.File, "test content 1");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task CopyObject_NotOverwrite(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutObjectAsync("test1.txt", "test content 1", false);
        await ufs.PutObjectAsync("test2.txt", "test content 2", false);

        // test
        Assert.That.ExpectException(async () => await ufs.CopyObjectAsync("test1.txt", "test2.txt", false));

        // verify
        UniversalFileSystemAssert.VerifyObject(ufs, "test1.txt", ObjectType.File, "test content 1");
        UniversalFileSystemAssert.VerifyObject(ufs, "test2.txt", ObjectType.File, "test content 2");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task CopyObject_CopyToItself(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutObjectAsync("test1.txt", "test content 1", false);

        // test
        Assert.That.ExpectException(async () => await ufs.CopyObjectAsync("test1.txt", "test1.txt", true));

        // verify
        UniversalFileSystemAssert.VerifyObject(ufs, "test1.txt", ObjectType.File, "test content 1");
    }
}