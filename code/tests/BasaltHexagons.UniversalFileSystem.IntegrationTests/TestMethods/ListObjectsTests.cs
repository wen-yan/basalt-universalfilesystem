using System;
using System.Threading.Tasks;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class ListObjectsTests
{
    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task ListObjects_BasicTest(IUniversalFileSystem ufs)
    {
        Console.WriteLine("ListObjects_BasicTest");
        await Task.CompletedTask;
    }
}
