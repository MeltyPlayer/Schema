using System;
using System.Runtime.CompilerServices;
using System.Text;

using CommunityToolkit.HighPerformance;

using schema.binary.attributes;

namespace schema.binary {
  public sealed partial class EndianBinaryReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertChar(char expectedValue)
      => EndianBinaryReader.Assert_(expectedValue, this.ReadChar());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public char ReadChar() => (char) this.ReadByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public char[] ReadChars(long count)
      => this.ReadChars(StringEncodingType.ASCII, count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadChars(char[] dst, int start, int length)
      => this.ReadChars(dst.AsSpan(start, length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadChars(Span<char> dst)
      => this.ReadChars(StringEncodingType.ASCII, dst);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertChar(StringEncodingType encodingType, char expectedValue)
      => EndianBinaryReader.Assert_(expectedValue, this.ReadChar(encodingType));

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
        this.BufferedStream_.BaseStream.Read(buffer.Slice(0, maxByteCount));

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
      while (!Eof) {
        var c = this.ReadChar(encodingType);
        if (c == endToken) {
          break;
        }

        strBuilder.Append(c);
      }

      return strBuilder.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadUpTo(params string[] endTokens)
      => ReadUpTo(StringEncodingType.ASCII, endTokens);

    public string ReadUpTo(StringEncodingType encodingType,
                           params string[] endTokens) {
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


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadLine() => ReadLine(StringEncodingType.ASCII);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadLine(StringEncodingType encodingType)
      => ReadUpTo(encodingType, "\n", "\r\n");


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertString(string expectedValue)
      => this.AssertString(StringEncodingType.ASCII, expectedValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadString(long count)
      => this.ReadString(StringEncodingType.ASCII, count);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertString(StringEncodingType encodingType,
                             string expectedValue)
      => EndianBinaryReader.Assert_(
          expectedValue.TrimEnd('\0'),
          this.ReadString(encodingType, expectedValue.Length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadString(StringEncodingType encodingType, long count)
      => new string(this.ReadChars(encodingType, count)).TrimEnd('\0');


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertStringNT(string expectedValue)
      => EndianBinaryReader.Assert_(expectedValue, this.ReadStringNT());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadStringNT() => ReadUpTo('\0');


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertStringNT(StringEncodingType encodingType,
                               string expectedValue)
      => EndianBinaryReader.Assert_(
          expectedValue,
          this.ReadStringNT(encodingType));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadStringNT(StringEncodingType encodingType)
      => ReadUpTo(encodingType, '\0');

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertMagicText(string expectedText) {
      var actualText = this.ReadString(expectedText.Length);

      if (expectedText != actualText) {
        throw new Exception(
            $"Expected to find magic text \"{expectedText}\", but found \"{actualText}\"");
      }
    }
  }
}