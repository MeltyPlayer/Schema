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

  public UnbufferedStreamReader(
      Stream impl,
      Encoding encoding,
      bool skipPreamble = true) {
    this.impl_ = impl;
    this.Encoding = encoding;
    this.decoder_ = encoding.GetDecoder();

    if (skipPreamble && this.Position == 0) {
      var preamble = encoding.GetPreamble();
      if (preamble.Length > 0 && this.Length >= preamble.Length) {
        Span<byte> header = stackalloc byte[preamble.Length];
        var read = impl.Read(header);
        if (read == preamble.Length && header.SequenceEqual(preamble)) {
          this.baseOffset_ = impl.Position;
          // Leave Position right after the BOM
        } else {
          impl.Position = 0;
        }
      }
    }
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