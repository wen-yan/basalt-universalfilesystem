using System;
using System.IO;

namespace BasaltHexagons.UniversalFileSystem.Core.IO;

public class PositionSupportedStream : DelegatedStream
{
    private long? _position;
    private long? _length;

    public PositionSupportedStream(Stream inner, long? position, long? length) : base(inner)
    {
        _position = position;
        _length = length;
    }

    public override long Position
    {
        get => _position ?? base.Position;
        set => throw new NotSupportedException();
    }

    public override long Length => _length ?? base.Length;

    public override long Seek(long offset, SeekOrigin origin)
    {
        long newPosition = base.Seek(offset, origin);
        if (_position != null)
            _position = newPosition;
        return newPosition;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int read = base.Read(buffer, offset, count);
        if (_position != null)
            _position += read;

        return read;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        base.Write(buffer, offset, count);
        if (_position != null)
            _position += count;
    }
}