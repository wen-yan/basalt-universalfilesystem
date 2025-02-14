using System;
using System.Threading.Tasks;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;

public abstract class GetObjectMetadataTests : FileSystemMethodTestsBase
{
    [TestMethod]
    public async Task GetObjectMetadata_BasicTest()
    {
        Console.WriteLine("GetObjectMetadata_BasicTest");
        await Task.CompletedTask;
    }
}