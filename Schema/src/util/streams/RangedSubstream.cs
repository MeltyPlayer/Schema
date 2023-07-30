using System;
using System.IO;

namespace schema.util.streams {
  /// <summary>
  /// Represents a substream of an underlying <see cref="Stream" />.
  /// </summary>
  public class RangedSubstream : Stream {
    private readonly Stream impl_;

    private readonly long offset = 0L;
    private readonly long length = 0L;

    /// <summary>
    /// Creates a new substream instance using the specified underlying stream at the specified offset with the specified length.
    /// </summary>
    /// <param name="stream">The underlying stream.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="length">The length.</param>
    public RangedSubstream(Stream stream, long offset, long length) {
      if (stream == null) {
        throw new ArgumentNullException(nameof(stream));
      }

      // Streams must support seeking for the concept of substreams to work.
      // At a pinch in the future we may support a poor man's seek (forward) by reading until the position is correct.

      if (!stream.CanSeek) {
        throw new NotSupportedException("Stream does not support seeking.");
      }

      this.impl_ = stream;

      if (offset < 0) {
        throw new ArgumentOutOfRangeException(nameof(offset),
                                              "Offset cannot be less than zero.");
      }

      if (length < 0) {
        throw new ArgumentOutOfRangeException(nameof(length),
                                              "Length cannot be less than zero.");
      }

      this.offset = offset;
      this.length = length;
    }

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count) {
      var startOffset = this.Position + offset;
      Asserts.True(startOffset >= this.offset,
                   "Attempted to read before the start of the substream!");

      return this.impl_.Read(
          buffer,
          offset,
          Convert.ToInt32(Math.Min(count,
                                   this.Position + this.length - startOffset)));
    }

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count) {
      var startOffset = this.Position + offset;
      Asserts.True(startOffset >= this.offset,
                   "Attempted to read before the start of the substream!");

      this.impl_.Write(
          buffer,
          offset,
          Convert.ToInt32(Math.Min(count,
                                   this.Position + this.length - startOffset)));
    }

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) {
      throw new NotSupportedException();
    }

    /// <inheritdoc />
    public override void SetLength(long value) {
      // While other Stream implementations allow the caller to set the length, this does not make much sense in the context of a substream.
      // Perhaps, in the future, we can allow callers to reduce the length, but not expand the length.

      throw new NotSupportedException(
          "Cannot set the length of a fixed substream.");
    }

    /// <inheritdoc />
    public override void Flush() => this.impl_.Flush();

    /// <inheritdoc />
    public override long Length => this.length;

    /// <inheritdoc />
    public override bool CanRead => this.impl_.CanRead;

    /// <inheritdoc />
    public override bool CanSeek => this.impl_.CanSeek;

    /// <inheritdoc />
    public override bool CanWrite => this.impl_.CanWrite;

    /// <inheritdoc />
    public override bool CanTimeout => this.impl_.CanTimeout;

    /// <inheritdoc />
    public override int ReadTimeout {
      get => this.impl_.ReadTimeout;
      set => throw new NotSupportedException(
          "Cannot set the read timeout of a substream.");
    }

    /// <inheritdoc />
    public override int WriteTimeout {
      get => this.impl_.WriteTimeout;
      set => throw new NotSupportedException(
          "Cannot set the write timeout of a substream.");
    }

    /// <inheritdoc />
    public override long Position {
      get => this.impl_.Position;
      set {
        Asserts.True(this.offset <= value,
                     "Attempted to seek before the start of the substream!");
        Asserts.True(value < this.offset + this.length,
                     "Attempted to seek past the end of the substream!");
        this.impl_.Position = value;
      }
    }
  }
}