using System;
using System.IO;
using System.Threading.Tasks;
using BasaltHexagons.UniversalFileSystem.Core.IO;
using Moq;

namespace BasaltHexagons.UniversalFileSystem.Core.UnitTests.IO;

[TestClass]
public class StreamWrapperTests
{
    [TestMethod]
    public async Task DisposeAsync_Test()
    {
        Mock<IAsyncDisposable> mockAsyncDisposable = new();
        Mock<IDisposable> mockDisposable = new();

        {
            await using Stream stream = new MemoryStream();
            await using StreamWrapper wrapper = new StreamWrapper(stream, [mockAsyncDisposable.Object], [mockDisposable.Object]);
        }

        mockAsyncDisposable.Verify(x => x.DisposeAsync(), Times.Once);
        mockDisposable.Verify(x => x.Dispose(), Times.Once);
    }

    [TestMethod]
    public void Dispose_Test()
    {
        Mock<IAsyncDisposable> mockAsyncDisposable = new();
        Mock<IDisposable> mockDisposable = new();

        {
            using Stream stream = new MemoryStream();
            using StreamWrapper wrapper = new StreamWrapper(stream, [mockAsyncDisposable.Object], [mockDisposable.Object]);
        }

        mockAsyncDisposable.Verify(x => x.DisposeAsync(), Times.Never);
        mockDisposable.Verify(x => x.Dispose(), Times.Once);
    }

    [TestMethod]
    public async Task StreamFunction_Test()
    {
        Mock<IAsyncDisposable> mockAsyncDisposable = new();
        Mock<IDisposable> mockDisposable = new();

        {
            await using Stream stream = new MemoryStream();
            await using (TextWriter writer = new StreamWriter(stream, leaveOpen: true))
            {
                await writer.WriteLineAsync("Hello World");
            }

            stream.Seek(0, SeekOrigin.Begin);
            await using StreamWrapper wrapper = new StreamWrapper(stream, [mockAsyncDisposable.Object], [mockDisposable.Object]);
            using TextReader reader = new StreamReader(wrapper, leaveOpen: true);

            string? result = await reader.ReadLineAsync();
            Assert.AreEqual("Hello World", result);
        }

        mockAsyncDisposable.Verify(x => x.DisposeAsync(), Times.Once);
        mockDisposable.Verify(x => x.Dispose(), Times.Once);
    }
}