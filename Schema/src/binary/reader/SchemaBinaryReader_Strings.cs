using System;
using System.Runtime.CompilerServices;
using System.Text;

using schema.binary.attributes;
using schema.text.reader;


namespace schema.binary;

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
  public void ReadChars(Span<char> dst) {
      // Reading a byte span and then copying them individually was found to be faster in benchmarking.
      Span<byte> bytes = stackalloc byte[dst.Length];
      this.ReadBytes(bytes);

      for (var i = 0; i < dst.Length; ++i) {
        dst[i] = (char) bytes[i];
      }
    }


  // Enum encoded Chars
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AssertChar(StringEncodingType encodingType, char expectedValue)
    => SchemaBinaryReader.Assert_(expectedValue, this.ReadChar(encodingType));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public char ReadChar(StringEncodingType encodingType)
    => this.ReadChar(encodingType.GetEncoding(this.Endianness));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public char[] ReadChars(StringEncodingType encodingType, long count)
    => this.ReadChars(encodingType.GetEncoding(this.Endianness), count);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ReadChars(StringEncodingType encodingType, Span<char> dst)
    => this.ReadChars(encodingType.GetEncoding(this.Endianness), dst);


  // Encoded Chars
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AssertChar(Encoding encoding, char expectedValue)
    => SchemaBinaryReader.Assert_(expectedValue, this.ReadChar(encoding));

  public unsafe char ReadChar(Encoding encoding) {
      char c;
      var ptr = &c;
      this.ReadChars(encoding, new Span<char>(ptr, 1));
      return c;
    }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public char[] ReadChars(Encoding encoding, long count) {
      var newArray = new char[count];
      this.ReadChars(encoding, newArray);
      return newArray;
    }

  public unsafe void ReadChars(Encoding encoding, Span<char> dst) {
      if (dst.Length == 0) {
        return;
      }

      var basePosition = this.Position;

      var maxByteCount = encoding.GetMaxByteCount(dst.Length);

      Span<byte> buffer = stackalloc byte[maxByteCount];
      var bufferPtr =
          (byte*) Unsafe.AsPointer(ref buffer.GetPinnableReference());
      var dstPtr = (char*) Unsafe.AsPointer(ref dst.GetPinnableReference());

      var decoder = encoding.GetDecoder();
      while (maxByteCount > 0) {
        this.Position = basePosition;
        this.bufferedStream_.BaseStream.TryToReadIntoBuffer(
            buffer.Slice(0, maxByteCount));

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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public string ReadUpTo(StringEncodingType encodingType, char endToken)
    => this.ReadUpTo(encodingType.GetEncoding(this.Endianness), endToken);

  public string ReadUpTo(Encoding encoding, char endToken) {
      var strBuilder = new StringBuilder();
      while (!this.Eof) {
        var c = this.ReadChar(encoding);
        if (c == endToken) {
          break;
        }

        strBuilder.Append(c);
      }

      return strBuilder.ToString();
    }

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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public string ReadUpTo(StringEncodingType encodingType,
                         ReadOnlySpan<string> endTokens)
    => this.ReadUpTo(encodingType.GetEncoding(this.Endianness), endTokens);

  public string ReadUpTo(Encoding encoding, ReadOnlySpan<string> endTokens) {
      var strBuilder = new StringBuilder();
      while (!Eof) {
        var firstC = this.ReadChar(encoding);
        var originalOffset = Position;

        foreach (var endToken in endTokens) {
          if (firstC == endToken[0]) {
            for (var i = 1; i < endToken.Length; ++i) {
              var c = this.ReadChar(encoding);
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
    => this.ReadLine(encodingType.GetEncoding(this.Endianness));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public string ReadLine(Encoding encoding)
    => this.ReadUpTo(encoding, TextReaderConstants.NEWLINE_STRINGS);


  // String
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AssertString(string expectedValue)
    => SchemaBinaryReader.AssertStrings_(
        expectedValue.AsSpan().TrimEnd('\0'),
        this.ReadString(expectedValue.Length).AsSpan());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AssertString(StringEncodingType encodingType,
                           string expectedValue)
    => this.AssertString(encodingType.GetEncoding(this.Endianness),
                         expectedValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AssertString(Encoding encoding, string expectedValue)
    => SchemaBinaryReader.AssertStrings_(
        expectedValue.AsSpan().TrimEnd('\0'),
        this.ReadString(encoding, expectedValue.Length).AsSpan());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public string ReadString(long count) {
      Span<char> buffer = stackalloc char[(int) count];
      this.ReadChars(buffer);
      return ((ReadOnlySpan<char>) buffer).TrimEnd('\0').ToString();
    }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public string ReadString(StringEncodingType encodingType, long count)
    => this.ReadString(encodingType.GetEncoding(this.Endianness), count);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public string ReadString(Encoding encoding, long count) {
      Span<char> buffer = stackalloc char[(int) count];
      this.ReadChars(encoding, buffer);
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
    => this.AssertStringNT(encodingType.GetEncoding(this.Endianness),
                           expectedValue);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public string ReadStringNT(StringEncodingType encodingType)
    => this.ReadStringNT(encodingType.GetEncoding(this.Endianness));


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AssertStringNT(Encoding encoding, string expectedValue)
    => SchemaBinaryReader.AssertStrings_(
        expectedValue.AsSpan(),
        // TODO: Consider removing an allocation here
        this.ReadStringNT(encoding).AsSpan());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public string ReadStringNT(Encoding encoding)
    => this.ReadUpTo(encoding, '\0');
}