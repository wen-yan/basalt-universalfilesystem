using System;
using System.IO;
using System.Threading.Tasks;

using BasaltHexagons.UniversalFileSystem.Core;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;

public abstract class GetObjectMetadataTests : FileSystemMethodTestsBase
{
    [TestMethod]
    public async Task GetObjectMetadata_BasicTest()
    {
        var ufs = this.GetUniversalFileSystem();

        // setup
        await using MemoryStream stream = new MemoryStream();
        await using(TextWriter writer = new StreamWriter(stream, leaveOpen: true))
        {
            writer.WriteLine("first test");
        }
        stream.Seek(0, SeekOrigin.Begin);

        await ufs.PutObjectAsync("test.txt", stream, true, default);

        // test
        ObjectMetadata metadata = await ufs.GetObjectMetadataAsync("test.txt", default);

        // Verify
        Assert.IsNotNull(metadata);
    }
}