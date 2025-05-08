using System;
using System.IO;
using System.Threading.Tasks;

namespace BasaltHexagons.UniversalFileSystem.Core.IO;

public class StreamWrapper : Stream
{
    private Stream _innerStream;
    private IAsyncDisposable[] _asyncDisposables;
    private IDisposable[] _disposables;
    private long? _contentLength;

    public StreamWrapper(Stream innerStream, IAsyncDisposable[] asyncDisposables, IDisposable[] disposables, long? contentLength = null)
    {
        _innerStream = innerStream;
        _asyncDisposables = asyncDisposables;
        _disposables = disposables;
        _contentLength = contentLength;
    }

    public override void Flush()
    {
        _innerStream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return _innerStream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _innerStream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        if (_contentLength.HasValue)
            throw new NotSupportedException();
        _innerStream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _innerStream.Write(buffer, offset, count);
    }

    public override bool CanRead => _innerStream.CanRead;

    public override bool CanSeek => _innerStream.CanSeek;

    public override bool CanWrite => _innerStream.CanWrite;

    public override long Length => _contentLength ?? _innerStream.Length;

    public override long Position
    {
        get => _innerStream.Position;
        set => _innerStream.Position = value;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        foreach (IDisposable disposable in _disposables)
            disposable.Dispose();
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        foreach (IAsyncDisposable disposable in _asyncDisposables)
            await disposable.DisposeAsync();
    }
}