using System.IO;
using System.Threading.Tasks;

namespace Basalt.UniversalFileSystem.Core.IO;

public abstract class DelegatedStream : Stream
{
    public DelegatedStream(Stream inner)
    {
        this.Inner = inner;
    }

    public Stream Inner { get; }

    public override void Flush() => this.Inner.Flush();
    public override int Read(byte[] buffer, int offset, int count) => this.Inner.Read(buffer, offset, count);
    public override long Seek(long offset, SeekOrigin origin) => this.Inner.Seek(offset, origin);
    public override void SetLength(long value) => this.Inner.SetLength(value);
    public override void Write(byte[] buffer, int offset, int count) => this.Inner.Write(buffer, offset, count);
    public override bool CanRead => this.Inner.CanRead;
    public override bool CanSeek => this.Inner.CanSeek;
    public override bool CanWrite => this.Inner.CanWrite;
    public override long Length => this.Inner.Length;

    public override long Position
    {
        get => this.Inner.Position;
        set => this.Inner.Position = value;
    }

    protected override void Dispose(bool disposing)
    {
        this.Inner.Dispose();
        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        await this.Inner.DisposeAsync();
        await base.DisposeAsync();
    }
}