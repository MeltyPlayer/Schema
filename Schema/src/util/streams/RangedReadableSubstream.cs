using System;
using System.IO;
using System.Runtime.CompilerServices;

using schema.util.asserts;


namespace schema.util.streams;

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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public byte ReadByte() {
      var startOffset = this.Position;
      if (startOffset >= this.offset_ + this.length_) {
        return unchecked((byte) -1);
      }

      Asserts.True(this.offset_ <= startOffset,
                   "Attempted to read before the start of the substream!");
      return this.impl_.ReadByte();
    }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int TryToReadIntoBuffer(Span<byte> dst) {
      var startOffset = this.Position;
      Asserts.True(this.offset_ <= startOffset,
                   "Attempted to read before the start of the substream!");

      var maxLength = Math.Min(dst.Length,
                               this.offset_ + this.length_ - startOffset);

      return this.impl_.TryToReadIntoBuffer(
          dst.Slice(0, Convert.ToInt32(maxLength)));
    }

  public long Position {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.impl_.Position;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set {
        Asserts.True(this.offset_ <= value,
                     "Attempted to seek before the start of the substream!");
        Asserts.True(value < this.offset_ + this.length_,
                     "Attempted to seek past the end of the substream!");
        this.impl_.Position = value;
      }
  }

  public long Length {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.offset_ + this.length_;
  }
}