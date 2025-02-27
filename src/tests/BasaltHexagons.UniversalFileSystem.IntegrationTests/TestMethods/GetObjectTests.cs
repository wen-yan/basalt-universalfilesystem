using System.Threading.Tasks;
using BasaltHexagons.UniversalFileSystem.TestUtils;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class GetObjectTests
{
    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetObject_FileInRoot(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutObjectAsync("test.txt", "test content", true);

        // test
        string content = await ufs.GetObjectAsync("test.txt");

        // verify
        Assert.AreEqual("test content", content);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetObject_FileInSubDirectory(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutObjectAsync("dir/test.txt", "test content", true);

        // test
        string content = await ufs.GetObjectAsync("dir/test.txt");

        // verify
        Assert.AreEqual("test content", content);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetObject_FileNotExists(UniversalFileSystemTestWrapper ufs)
    {
        // test
        Assert.That.ExpectException(async () => await ufs.GetObjectAsync("test.txt"));
        await Task.CompletedTask;
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetObject_SameNameAsDirectory(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutObjectAsync("dir/test.txt", "test content", true);

        // test
        Assert.That.ExpectException(async () => await ufs.GetObjectAsync("dir"));
    }
}