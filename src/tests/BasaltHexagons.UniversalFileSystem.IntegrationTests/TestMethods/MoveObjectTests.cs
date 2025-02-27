using System.Threading.Tasks;
using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.TestUtils;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class MoveObjectTests
{
    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task MoveObject_ToSameDirectory(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutObjectAsync("test.txt", "test content", false);

        // test
        await ufs.MoveObjectAsync("test.txt", "test2.txt", false);

        // verify
        Assert.IsFalse(await ufs.ExistsAsync("test.txt"));
        UniversalFileSystemAssert.VerifyObject(ufs, "test2.txt", ObjectType.File, "test content");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task MoveObject_ToDifferentDirectory(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutObjectAsync("test.txt", "test content", false);

        // test
        await ufs.MoveObjectAsync("test.txt", "dir/test.txt", false);

        // verify
        Assert.IsFalse(await ufs.ExistsAsync("test.txt"));
        UniversalFileSystemAssert.VerifyObject(ufs, "dir/test.txt", ObjectType.File, "test content");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task MoveObject_Overwrite(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutObjectAsync("test.txt", "test content", false);
        await ufs.PutObjectAsync("test2.txt", "test content2", false);

        // test
        await ufs.MoveObjectAsync("test.txt", "test2.txt", true);

        // verify
        Assert.IsFalse(await ufs.ExistsAsync("test.txt"));
        UniversalFileSystemAssert.VerifyObject(ufs, "test2.txt", ObjectType.File, "test content");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task MoveObject_NotOverwrite(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutObjectAsync("test.txt", "test content", false);
        await ufs.PutObjectAsync("test2.txt", "test content2", false);

        // test
        Assert.That.ExpectException(async () => await ufs.MoveObjectAsync("test.txt", "test2.txt", false));

        // verify
        UniversalFileSystemAssert.VerifyObject(ufs, "test.txt", ObjectType.File, "test content");
        UniversalFileSystemAssert.VerifyObject(ufs, "test2.txt", ObjectType.File, "test content2");
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task MoveObject_MoveToItself(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutObjectAsync("test.txt", "test content", false);

        // test
        Assert.That.ExpectException(async () => await ufs.MoveObjectAsync("test.txt", "test.txt", true));

        // verify
        UniversalFileSystemAssert.VerifyObject(ufs, "test.txt", ObjectType.File, "test content");
    }
}