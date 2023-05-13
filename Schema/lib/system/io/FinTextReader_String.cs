using System.Linq;
using System.Text;

using schema.binary.util;

namespace System.IO {
  public sealed partial class FinTextReader {
    public void AssertChar(char expectedValue)
      => Asserts.Equal(expectedValue, this.ReadChar());

    public char ReadChar() => (char) this.baseStream_.ReadByte();
    public char[] ReadChars(long count) => ReadChars(new char[count]);

    public char[] ReadChars(char[] dst) {
      for (var i = 0; i < dst.Length; ++i) {
        dst[i] = ReadChar();
      }

      return dst;
    }


    public void AssertString(string expectedValue)
      => Asserts.Equal(expectedValue, this.ReadString(expectedValue.Length));

    public string ReadString(long count) {
      var sb = new StringBuilder((int) count);
      for (var i = 0; i < count; ++i) {
        sb.Append(ReadChar());
      }

      return sb.ToString();
    }

    public string[] ReadStrings(string[] separators, string[] terminators)
      => this.ReadSplitUpToAndPastTerminatorsIncludingEmpty_(separators, terminators)
             .ToArray();


    public string ReadLine()
      => this.ReadUpToAndPastTerminator(TextReaderConstants.NEWLINE_STRINGS);
  }
}