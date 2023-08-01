using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using schema.binary.attributes;


namespace System.IO {
  public sealed partial class EndianBinaryWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteChar(char value)
      => this.WriteChar(StringEncodingType.ASCII, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteChars(char[] values, int offset, int count)
      => this.WriteChars(StringEncodingType.ASCII, values, offset, count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteChars(ReadOnlySpan<char> values)
      => this.WriteChars(StringEncodingType.ASCII, values);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteChars(IReadOnlyList<char> values)
      => this.WriteChars(StringEncodingType.ASCII, values);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteChars(IReadOnlyList<char> values, int offset, int count)
      => this.WriteChars(StringEncodingType.ASCII, values, offset, count);


    public unsafe void WriteChar(StringEncodingType encodingType, char value) {
      var ptr = &value;
      var srcSpan = new Span<char>(ptr, 1);

      var encoding = encodingType.GetEncoding(this.Endianness);
      var byteCount = encoding.GetByteCount(srcSpan);
      Span<byte> dstSpan = stackalloc byte[byteCount];
      encoding.GetBytes(srcSpan, dstSpan);

      this.WriteBytes(dstSpan);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteChars(StringEncodingType encodingType,
                           char[] values,
                           int offset,
                           int count)
      => this.WriteChars(encodingType, values.AsSpan(offset, count));

    public void WriteChars(StringEncodingType encodingType,
                           ReadOnlySpan<char> values) {
      if (values.Length == 0) {
        return;
      }

      var encoding = encodingType.GetEncoding(this.Endianness);
      var byteCount = encoding.GetByteCount(values);
      Span<byte> dstSpan = stackalloc byte[byteCount];
      encoding.GetBytes(values, dstSpan);
      this.WriteBytes(dstSpan);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteChars(StringEncodingType encodingType,
                           IReadOnlyList<char> values)
      => this.WriteChars(encodingType, values, 0, values.Count);

    public void WriteChars(StringEncodingType encodingType,
                           IReadOnlyList<char> values,
                           int offset,
                           int count) {
      for (var i = offset; i < offset + count; ++i) {
        this.WriteChar(encodingType, values[i]);
      }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteString(string value)
      => this.WriteString(StringEncodingType.ASCII, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteString(StringEncodingType encodingType, string value)
      => this.WriteChars(encodingType, value.AsSpan());


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteStringNT(string value)
      => this.WriteStringNT(StringEncodingType.ASCII, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteStringNT(StringEncodingType encodingType, string value) {
      this.WriteString(encodingType, value);
      this.WriteChar(encodingType, '\0');
    }

    public void WriteStringWithExactLength(string value, int length)
      => this.WriteStringWithExactLength(StringEncodingType.ASCII,
                                         value,
                                         length);

    public unsafe void WriteStringWithExactLength(
        StringEncodingType encodingType,
        string value,
        int length) {
      var difference = length - value.Length;

      if (difference < 0) {
        this.WriteChars(encodingType, value.AsSpan(0, length));
        return;
      }

      this.WriteString(encodingType, value);

      if (difference == 0) {
        return;
      }

      var src = '\0';
      var srcSpan = new Span<char>(&src, 1);
      var encoding = encodingType.GetEncoding(this.Endianness);
      var byteCount = encoding.GetByteCount(srcSpan);
      Span<byte> dstSpan = stackalloc byte[byteCount];
      encoding.GetBytes(srcSpan, dstSpan);
      for (var i = 0; i < difference; ++i) {
        this.WriteBytes(dstSpan);
      }
    }
  }
}