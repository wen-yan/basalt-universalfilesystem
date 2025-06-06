namespace Basalt.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class DoesFileExistTests
{
    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task DoesFileExist(IUniversalFileSystem ufs, UriWrapper u)
    {
        // setup
        await ufs.PutFileAsync(u.GetFullUri("test1.txt"), "test content", false);
        await ufs.PutFileAsync(u.GetFullUri("dir/test2.txt"), "test2 content", false);

        // verify
        Assert.IsTrue(await ufs.DoesFileExistAsync(u.GetFullUri("test1.txt")));
        Assert.IsTrue(await ufs.DoesFileExistAsync(u.GetFullUri("dir/test2.txt")));
        Assert.IsFalse(await ufs.DoesFileExistAsync(u.GetFullUri("test2.txt")));
        Assert.IsFalse(await ufs.DoesFileExistAsync(u.GetFullUri("dir/")));
        Assert.IsFalse(await ufs.DoesFileExistAsync(u.GetFullUri("dir")));
    }
}