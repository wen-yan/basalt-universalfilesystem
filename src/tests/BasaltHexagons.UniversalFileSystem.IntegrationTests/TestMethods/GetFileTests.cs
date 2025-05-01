using System.Threading.Tasks;
using BasaltHexagons.UniversalFileSystem.Core.Exceptions;
using BasaltHexagons.UniversalFileSystem.TestUtils;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class GetFileTests
{
    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetFile_FileInRoot(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("test.txt", "test content", true);

        // test
        string content = await ufs.GetFileAsync("test.txt");

        // verify
        Assert.AreEqual("test content", content);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetFile_FileInSubDirectory(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("dir/test.txt", "test content", true);

        // test
        string content = await ufs.GetFileAsync("dir/test.txt");

        // verify
        Assert.AreEqual("test content", content);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetFile_FileNotExists(UniversalFileSystemTestWrapper ufs)
    {
        // test
        Assert.That.ExpectException<FileNotExistsException>(async () => await ufs.GetFileAsync("test.txt"));
        await Task.CompletedTask;
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetFile_SameNameAsDirectory(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("dir/test.txt", "test content", true);

        // test
        Assert.That.ExpectException<FileNotExistsException>(async () => await ufs.GetFileAsync("dir"));
    }
}