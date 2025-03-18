using System.Threading.Tasks;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class DoesFileExistTests
{
    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task DoesFileExist(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("test1.txt", "test content", false);
        await ufs.PutFileAsync("dir/test2.txt", "test2 content", false);

        // verify
        Assert.IsTrue(await ufs.DoesFileExistAsync("test1.txt"));
        Assert.IsTrue(await ufs.DoesFileExistAsync("dir/test2.txt"));
        Assert.IsFalse(await ufs.DoesFileExistAsync("test2.txt"));
        Assert.IsFalse(await ufs.DoesFileExistAsync("dir/"));
        Assert.IsFalse(await ufs.DoesFileExistAsync("dir"));
    }
}