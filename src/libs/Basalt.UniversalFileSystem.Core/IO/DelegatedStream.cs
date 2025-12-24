using System.IO;
using System.Threading.Tasks;

namespace Basalt.UniversalFileSystem.Core.IO;

/// <summary>
/// Abstract class for delegated stream implementations.
/// </summary>
public abstract class DelegatedStream : Stream
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="inner">Inner Stream object.</param>
    protected DelegatedStream(Stream inner)
    {
        this.Inner = inner;
    }

    /// <summary>
    /// Inner Stream object.
    /// </summary>
    public Stream Inner { get; }

    /// <inheritdoc />
    public override void Flush() => this.Inner.Flush();

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count) => this.Inner.Read(buffer, offset, count);

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) => this.Inner.Seek(offset, origin);

    /// <inheritdoc />
    public override void SetLength(long value) => this.Inner.SetLength(value);

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count) => this.Inner.Write(buffer, offset, count);

    /// <inheritdoc />
    public override bool CanRead => this.Inner.CanRead;

    /// <inheritdoc />
    public override bool CanSeek => this.Inner.CanSeek;

    /// <inheritdoc />
    public override bool CanWrite => this.Inner.CanWrite;

    /// <inheritdoc />
    public override long Length => this.Inner.Length;

    /// <inheritdoc />
    public override long Position
    {
        get => this.Inner.Position;
        set => this.Inner.Position = value;
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        this.Inner.Dispose();
        base.Dispose(disposing);
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        await this.Inner.DisposeAsync().ConfigureAwait(false);
        await base.DisposeAsync().ConfigureAwait(false);
    }
}