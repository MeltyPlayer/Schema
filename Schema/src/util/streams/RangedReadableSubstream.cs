using System;
using System.IO;

namespace schema.util.streams {
  /// <summary>
  /// Represents a substream of an underlying <see cref="Stream" />.
  /// </summary>
  public class RangedReadableSubstream : ISeekableReadableStream {
    private readonly ISeekableReadableStream impl_;

    private readonly long offset_ = 0L;
    private readonly long length_ = 0L;

    public RangedReadableSubstream(ISeekableReadableStream stream,
                                   long offset,
                                   long length) {
      this.impl_ = stream ?? throw new ArgumentNullException(nameof(stream));

      if (offset < 0) {
        throw new ArgumentOutOfRangeException(nameof(offset),
                                              "Offset cannot be less than zero.");
      }

      if (length < 0) {
        throw new ArgumentOutOfRangeException(nameof(length),
                                              "Length cannot be less than zero.");
      }

      this.offset_ = offset;
      this.length_ = length;
    }

    public void Dispose() {
      // Should not dispose of impl, since it may still need to be used
      // afterwards.
    }

    public int Read(Span<byte> dst) {
      var startOffset = this.Position;
      Asserts.True(startOffset >= this.offset_,
                   "Attempted to read before the start of the substream!");

      var maxLength = Math.Min(dst.Length,
                               this.offset_ + this.length_ - startOffset);

      return this.impl_.Read(dst.Slice(0, Convert.ToInt32(maxLength)));
    }

    public long Position {
      get => this.impl_.Position;
      set {
        Asserts.True(this.offset_ <= value,
                     "Attempted to seek before the start of the substream!");
        Asserts.True(value < this.offset_ + this.length_,
                     "Attempted to seek past the end of the substream!");
        this.impl_.Position = value;
      }
    }

    public long Length => this.offset_ + this.length_;
  }
}