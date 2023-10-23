using System;
using System.Runtime.CompilerServices;
using System.Text;

using schema.binary.attributes;
using schema.text.reader;

namespace schema.binary {
  public sealed partial class SchemaBinaryReader {
    // ASCII Chars
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertChar(char expectedValue)
      => SchemaBinaryReader.Assert_(expectedValue, this.ReadChar());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public char ReadChar() => (char) this.ReadByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public char[] ReadChars(long count) {
      var newArray = new char[count];
      this.ReadChars(newArray);
      return newArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadChars(char[] dst, int start, int length)
      => this.ReadChars(dst.AsSpan(start, length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadChars(Span<char> dst) {
      // Reading a byte span and then copying them individually was found to be faster in benchmarking.
      Span<byte> bytes = stackalloc byte[dst.Length];
      this.ReadBytes(bytes);

      for (var i = 0; i < dst.Length; ++i) {
        dst[i] = (char) bytes[i];
      }
    }


    // Encoded Chars
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertChar(StringEncodingType encodingType, char expectedValue)
      => SchemaBinaryReader.Assert_(expectedValue, this.ReadChar(encodingType));

    public unsafe char ReadChar(StringEncodingType encodingType) {
      char c;
      var ptr = &c;
      this.ReadChars(encodingType, new Span<char>(ptr, 1));
      return c;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public char[] ReadChars(StringEncodingType encodingType, long count) {
      var newArray = new char[count];
      this.ReadChars(encodingType, newArray);
      return newArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadChars(StringEncodingType encodingType,
                          char[] dst,
                          int start,
                          int length)
      => this.ReadChars(encodingType, dst.AsSpan(start, length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void ReadChars(StringEncodingType encodingType,
                                 Span<char> dst) {
      if (dst.Length == 0) {
        return;
      }

      var basePosition = this.Position;

      var encoding = encodingType.GetEncoding(this.Endianness);
      var maxByteCount = encoding.GetMaxByteCount(dst.Length);

      Span<byte> buffer = stackalloc byte[maxByteCount];
      var bufferPtr =
          (byte*) Unsafe.AsPointer(ref buffer.GetPinnableReference());
      var dstPtr = (char*) Unsafe.AsPointer(ref dst.GetPinnableReference());

      var decoder = encoding.GetDecoder();
      while (maxByteCount > 0) {
        this.Position = basePosition;
        this.bufferedStream_.BaseStream.Read(buffer.Slice(0, maxByteCount));

        decoder.Convert(bufferPtr,
                        maxByteCount,
                        dstPtr,
                        dst.Length,
                        false,
                        out var bytesUsed,
                        out var charsUsed,
                        out var completed);

        if (charsUsed == dst.Length) {
          this.Position = basePosition + bytesUsed;
          encoding.GetChars(buffer.Slice(0, bytesUsed), dst);
          return;
        }

        --maxByteCount;
      }

      this.Position = basePosition;
    }


    // Read Up To
    public string ReadUpTo(char endToken) {
      var remainingCharacters = this.Length - this.Position;

      var strBuilder = new StringBuilder();
      char c;
      while ((remainingCharacters--) > 0 && (c = this.ReadChar()) != endToken) {
        strBuilder.Append(c);
      }

      return strBuilder.ToString();
    }

    public string ReadUpTo(StringEncodingType encodingType, char endToken) {
      var strBuilder = new StringBuilder();
      while (!this.Eof) {
        var c = this.ReadChar(encodingType);
        if (c == endToken) {
          break;
        }

        strBuilder.Append(c);
      }

      return strBuilder.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadUpTo(ReadOnlySpan<string> endTokens) {
      var strBuilder = new StringBuilder();
      while (!Eof) {
        var firstC = this.ReadChar();
        var originalOffset = Position;

        foreach (var endToken in endTokens) {
          if (firstC == endToken[0]) {
            for (var i = 1; i < endToken.Length; ++i) {
              var c = this.ReadChar();
              if (c != endToken[1]) {
                Position = originalOffset;
                break;
              }
            }

            goto Done;
          }
        }

        strBuilder.Append(firstC);
      }

      Done:
      return strBuilder.ToString();
    }

    public string ReadUpTo(StringEncodingType encodingType,
                           ReadOnlySpan<string> endTokens) {
      var strBuilder = new StringBuilder();
      while (!Eof) {
        var firstC = this.ReadChar(encodingType);
        var originalOffset = Position;

        foreach (var endToken in endTokens) {
          if (firstC == endToken[0]) {
            for (var i = 1; i < endToken.Length; ++i) {
              var c = this.ReadChar(encodingType);
              if (c != endToken[1]) {
                Position = originalOffset;
                break;
              }
            }

            goto Done;
          }
        }

        strBuilder.Append(firstC);
      }

      Done:
      return strBuilder.ToString();
    }


    // Read Line

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadLine()
      => this.ReadUpTo(TextReaderConstants.NEWLINE_STRINGS);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadLine(StringEncodingType encodingType)
      => this.ReadUpTo(encodingType, TextReaderConstants.NEWLINE_STRINGS);


    // String
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertString(string expectedValue)
      => SchemaBinaryReader.AssertStrings_(
          expectedValue.AsSpan().TrimEnd('\0'),
          this.ReadString(expectedValue.Length).AsSpan());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertString(StringEncodingType encodingType,
                             string expectedValue)
      => SchemaBinaryReader.AssertStrings_(
          expectedValue.AsSpan().TrimEnd('\0'),
          this.ReadString(encodingType, expectedValue.Length).AsSpan());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadString(long count) {
      Span<char> buffer = stackalloc char[(int) count];
      this.ReadChars(buffer);
      return ((ReadOnlySpan<char>) buffer).TrimEnd('\0').ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadString(StringEncodingType encodingType, long count) {
      Span<char> buffer = stackalloc char[(int) count];
      this.ReadChars(encodingType, buffer);
      return ((ReadOnlySpan<char>) buffer).TrimEnd('\0').ToString();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertStringNT(string expectedValue)
      => SchemaBinaryReader.AssertStrings_(
          expectedValue.AsSpan(),
          // TODO: Consider removing an allocation here
          this.ReadStringNT().AsSpan());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadStringNT() => this.ReadUpTo('\0');


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertStringNT(StringEncodingType encodingType,
                               string expectedValue)
      => SchemaBinaryReader.AssertStrings_(
          expectedValue.AsSpan(),
          // TODO: Consider removing an allocation here
          this.ReadStringNT(encodingType).AsSpan());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadStringNT(StringEncodingType encodingType)
      => this.ReadUpTo(encodingType, '\0');
  }
}