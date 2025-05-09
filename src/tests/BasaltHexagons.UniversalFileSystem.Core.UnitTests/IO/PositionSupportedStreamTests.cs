using System.IO;
using System.Threading.Tasks;
using BasaltHexagons.UniversalFileSystem.Core.IO;

namespace BasaltHexagons.UniversalFileSystem.Core.UnitTests.IO;

[TestClass]
public class PositionSupportedStreamTests
{
    [TestMethod]
    public async Task Length_Test()
    {
        await using Stream inner = new MemoryStream();
        await using PositionSupportedStream stream = new PositionSupportedStream(inner, null, 10);
        Assert.AreEqual(10, stream.Length);
    }

    [TestMethod]
    public async Task NullLength_Test()
    {
        byte[] content = new byte[10];
        await using Stream inner = new MemoryStream();
        await inner.WriteAsync(content, 0, content.Length);
        inner.Seek(0, SeekOrigin.Begin);

        await using PositionSupportedStream stream = new PositionSupportedStream(inner, null, null);
        Assert.AreEqual(content.Length, stream.Length);
    }

    [TestMethod]
    public async Task Position_Test()
    {
        byte[] content = new byte[10];
        await using Stream inner = new MemoryStream();
        await inner.WriteAsync(content, 0, content.Length);
        inner.Seek(0, SeekOrigin.Begin);

        await using PositionSupportedStream stream = new PositionSupportedStream(inner, 0, null);
        Assert.AreEqual(0, stream.Position);

        stream.ReadByte();
        Assert.AreEqual(1, stream.Position);
    }

    [TestMethod]
    public async Task NullPosition_Test()
    {
        byte[] content = new byte[10];
        await using Stream inner = new MemoryStream();
        await inner.WriteAsync(content, 0, content.Length);
        inner.Seek(0, SeekOrigin.Begin);

        await using PositionSupportedStream stream = new PositionSupportedStream(inner, null, null);
        Assert.AreEqual(0, stream.Position);

        stream.ReadByte();
        Assert.AreEqual(1, stream.Position);
    }
}