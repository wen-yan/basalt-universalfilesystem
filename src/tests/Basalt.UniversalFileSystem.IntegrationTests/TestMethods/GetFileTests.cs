using Basalt.UniversalFileSystem.Core.Exceptions;
using Basalt.UniversalFileSystem.TestUtils;

namespace Basalt.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class GetFileTests
{
    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetFile_FileInRoot(IUniversalFileSystem ufs, UriWrapper u)
    {
        // setup
        await ufs.PutFileAsync(u.GetFullUri("test.txt"), "test content", true);

        // test
        string content = await ufs.GetFileStringAsync(u.GetFullUri("test.txt"));

        // verify
        Assert.AreEqual("test content", content);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetFile_FileInSubDirectory(IUniversalFileSystem ufs, UriWrapper u)
    {
        // setup
        await ufs.PutFileAsync(u.GetFullUri("dir/test.txt"), "test content", true);

        // test
        string content = await ufs.GetFileStringAsync(u.GetFullUri("dir/test.txt"));

        // verify
        Assert.AreEqual("test content", content);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetFile_FileNotExists(IUniversalFileSystem ufs, UriWrapper u)
    {
        // test
        await Assert.That.ExpectException<FileNotExistsException>(async () => await ufs.GetFileAsync(u.GetFullUri("test.txt")));
        await Task.CompletedTask;
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task GetFile_SameNameAsDirectory(IUniversalFileSystem ufs, UriWrapper u)
    {
        // setup
        await ufs.PutFileAsync(u.GetFullUri("dir/test.txt"), "test content", true);

        // test
        await Assert.That.ExpectException<FileNotExistsException>(async () => await ufs.GetFileAsync(u.GetFullUri("dir")));
    }
}