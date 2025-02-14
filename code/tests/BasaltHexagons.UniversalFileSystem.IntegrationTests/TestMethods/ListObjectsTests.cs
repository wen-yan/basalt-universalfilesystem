using System;
using System.Threading.Tasks;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;


public class ListObjectsTests : FileSystemMethodTestsBase
{
    [TestMethod]
    public async Task ListObjects_BasicTest()
    {
        IUniversalFileSystem fs = this.GetUniversalFileSystem();
        Console.WriteLine("ListObjects_BasicTest");
        await Task.CompletedTask;
    }
}
