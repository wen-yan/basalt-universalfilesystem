using System;
using System.IO;

namespace Basalt.UniversalFileSystem.Core.IO;

/// <summary>
/// DelegatedStream supporting position.
/// </summary>
public class PositionSupportedStream : DelegatedStream
{
    private long? _position;
    private long? _length;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="inner">Inner Stream object.</param>
    /// <param name="position">Current cursor position.</param>
    /// <param name="length">Content length.</param>
    public PositionSupportedStream(Stream inner, long? position, long? length) : base(inner)
    {
        _position = position;
        _length = length;
    }

    /// <inheritdoc />
    public override long Position
    {
        get => _position ?? base.Position;
        set => throw new NotSupportedException();
    }

    /// <inheritdoc />
    public override long Length => _length ?? base.Length;

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin)
    {
        long newPosition = base.Seek(offset, origin);
        if (_position != null)
            _position = newPosition;
        return newPosition;
    }

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count)
    {
        int read = base.Read(buffer, offset, count);
        if (_position != null)
            _position += read;

        return read;
    }

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count)
    {
        base.Write(buffer, offset, count);
        if (_position != null)
            _position += count;
    }
}