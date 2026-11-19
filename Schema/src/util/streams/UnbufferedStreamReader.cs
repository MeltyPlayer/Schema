using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

using CommunityToolkit.HighPerformance;

using schema.util.asserts;


namespace schema.util.streams;

public sealed class UnbufferedStreamReader : IDisposable {
  private readonly Stream impl_;
  private readonly Decoder decoder_;
  private readonly long baseOffset_ = 0;

  public Encoding Encoding { get; }

  public UnbufferedStreamReader(Stream impl, Encoding? fallbackEncoding) {
    this.impl_ = impl;

    this.Encoding = GetEncoding_(impl, fallbackEncoding);
    this.baseOffset_ = impl.Position;

    this.decoder_ = this.Encoding.GetDecoder();
  }

  private static Encoding GetEncoding_(
      Stream stream,
      Encoding? fallbackEncoding = null) {
    long originalPosition = stream.Position;
    Span<byte> bom = stackalloc byte[4];
    int bytesRead = stream.Read(bom);

    if (bytesRead >= 4) {
      if (bom[0] == 0x00 &&
          bom[1] == 0x00 &&
          bom[2] == 0xFE &&
          bom[3] == 0xFF) {
        stream.Position = originalPosition + 4;
        return new UTF32Encoding(bigEndian: true, byteOrderMark: true);
      }

      if (bom[0] == 0xFF &&
          bom[1] == 0xFE &&
          bom[2] == 0x00 &&
          bom[3] == 0x00) {
        stream.Position = originalPosition + 4;
        return Encoding.UTF32;
      }
    }

    if (bytesRead >= 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF) {
      stream.Position = originalPosition + 3;
      return Encoding.UTF8;
    }

    if (bytesRead >= 2) {
      if (bom[0] == 0xFE && bom[1] == 0xFF) {
        stream.Position = originalPosition + 2;
        return Encoding.BigEndianUnicode;
      }

      if (bom[0] == 0xFF && bom[1] == 0xFE) {
        stream.Position = originalPosition + 2;
        return Encoding.Unicode;
      }
    }

    stream.Position = originalPosition;
    return fallbackEncoding ??
           new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
  }

  public void Dispose() => this.impl_.Dispose();

  public long Position {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.impl_.Position - this.baseOffset_;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set => this.impl_.Position = this.baseOffset_ + value;
  }

  public long Length {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.impl_.Length - this.baseOffset_;
  }

  public bool Eof {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Position >= this.Length;
  }

  public int MaxRemainingChars {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Encoding.GetMaxCharCount((int) (this.Length - this.Position));
  }

  public char ReadChar() {
    Asserts.Equal(true, this.TryReadChar(out var c));
    return c;
  }

  public unsafe bool TryReadChar(out char c) {
    fixed (char* ptr = &c) {
      return this.ReadChars(new Span<char>(ptr, 1)) == 1;
    }
  }

  public unsafe int ReadChars(Span<char> dst) {
    if (dst.Length == 0) {
      return 0;
    }

    this.decoder_.Reset();

    var basePosition = this.Position;
    var maxByteCount = this.Encoding.GetMaxByteCount(dst.Length);

    // Clamp to remaining stream length to avoid reading past EOF
    var remainingBytes = this.Length - basePosition;
    if (remainingBytes <= 0) {
      return 0;
    }

    if (maxByteCount > remainingBytes) {
      maxByteCount = (int) remainingBytes;
    }

    Span<byte> buffer = stackalloc byte[maxByteCount];
    var bytesRead = this.impl_.Read(buffer);
    if (bytesRead == 0) {
      return 0;
    }

    fixed (byte* bufferPtr = buffer)
    fixed (char* dstPtr = dst) {
      var totalBytesUsed = 0;
      var totalCharsRead = 0;

      // Feed 1 byte at a time so Decoder.Convert never over-consumes 
      // past the exact byte boundary of dst.Length characters.
      while (totalCharsRead < dst.Length && totalBytesUsed < bytesRead) {
        this.decoder_.Convert(
            bufferPtr + totalBytesUsed,
            1,
            dstPtr + totalCharsRead,
            dst.Length - totalCharsRead,
            flush: totalBytesUsed == bytesRead - 1,
            out var bytesUsed,
            out var charsUsed,
            out _);

        totalBytesUsed += bytesUsed;
        totalCharsRead += charsUsed;
      }

      this.Position = basePosition + totalBytesUsed;

      return totalCharsRead;
    }
  }
}