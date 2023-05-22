using System.Runtime.CompilerServices;
using System.Text;

namespace System.IO {
  public sealed partial class EndianBinaryReader {
    // TODO: Handle other encodings besides ASCII

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertChar(char expectedValue)
      => EndianBinaryReader.Assert_(expectedValue, this.ReadChar());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public char ReadChar() => (char) this.ReadByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public char[] ReadChars(long count)
      => this.ReadChars(Encoding.ASCII, count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadChars(char[] dst, int start, int length)
      => this.ReadChars(dst.AsSpan(start, length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadChars(Span<char> dst)
      => this.ReadChars(Encoding.ASCII, dst);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertChar(Encoding encoding, char expectedValue)
      => EndianBinaryReader.Assert_(expectedValue, this.ReadChar(encoding));

    public unsafe char ReadChar(Encoding encoding) {
      var encodingSize = EndianBinaryReader.GetEncodingSize_(encoding);
      Span<byte> bBuffer = stackalloc byte[encodingSize];
      this.BufferedStream_.FillBuffer(bBuffer, encodingSize);

      Span<char> cBuffer = stackalloc char[1];

      encoding.GetChars(bBuffer, cBuffer);

      return cBuffer[0];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public char[] ReadChars(Encoding encoding, long count) {
      var newArray = new char[count];
      this.ReadChars(encoding, newArray);
      return newArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadChars(Encoding encoding,
                          char[] dst,
                          int start,
                          int length)
      => this.ReadChars(encoding, dst.AsSpan(start, length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadChars(Encoding encoding, Span<char> dst) {
      var encodingSize = EndianBinaryReader.GetEncodingSize_(encoding);
      var lengthInBytes = encodingSize * dst.Length;
      this.BufferedStream_.FillBuffer(lengthInBytes, encodingSize);
      encoding.GetChars(this.BufferedStream_.Buffer.AsSpan(0, lengthInBytes), dst);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetEncodingSize_(Encoding encoding) {
      return encoding == Encoding.UTF8 ||
             encoding == Encoding.ASCII ||
             encoding != Encoding.Unicode &&
             encoding != Encoding.BigEndianUnicode
          ? 1
          : 2;
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

    public string ReadUpTo(Encoding encoding, char endToken) {
      var strBuilder = new StringBuilder();
      while (!Eof) {
        var c = this.ReadChar(encoding);
        if (c == endToken) {
          break;
        }

        strBuilder.Append(c);
      }

      return strBuilder.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadUpTo(params string[] endTokens)
      => ReadUpTo(Encoding.ASCII, endTokens);

    public string ReadUpTo(Encoding encoding, params string[] endTokens) {
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


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadLine() => ReadLine(Encoding.ASCII);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadLine(Encoding encoding)
      => ReadUpTo(encoding, "\n", "\r\n");


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertString(string expectedValue)
      => this.AssertString(Encoding.ASCII, expectedValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadString(long count)
      => this.ReadString(Encoding.ASCII, count);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertString(Encoding encoding, string expectedValue)
      => EndianBinaryReader.Assert_(
          expectedValue.TrimEnd('\0'),
          this.ReadString(encoding, expectedValue.Length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadString(Encoding encoding, long count)
      => new string(this.ReadChars(encoding, count)).TrimEnd('\0');


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertStringNT(string expectedValue)
      => EndianBinaryReader.Assert_(expectedValue, this.ReadStringNT());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadStringNT() => ReadUpTo('\0');


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertStringNT(Encoding encoding, string expectedValue)
      => EndianBinaryReader.Assert_(
          expectedValue,
          this.ReadStringNT(encoding));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadStringNT(Encoding encoding) => ReadUpTo(encoding, '\0');

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