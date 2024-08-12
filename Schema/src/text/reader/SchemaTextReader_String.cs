using System;
using System.Linq;
using System.Text;

using schema.util.asserts;


namespace schema.text.reader {
  public sealed partial class SchemaTextReader {
    public void AssertChar(char expectedValue)
      => Asserts.Equal(expectedValue, this.ReadChar());

    public char[] ReadChars(long count) {
      var newArray = new char[count];
      this.ReadChars(newArray);
      return newArray;
    }

    // TODO: Handle other encodings besides ASCII
    public void ReadChars(Span<char> dst) {
      for (var i = 0; i < dst.Length; ++i) {
        dst[i] = this.ReadChar();
      }
    }

    public void AssertString(string expectedValue)
      => Asserts.Equal(expectedValue, this.ReadString(expectedValue.Length));

    public string ReadString(long count) {
      var sb = new StringBuilder((int) count);
      for (var i = 0; i < count; ++i) {
        sb.Append(this.ReadChar());
      }

      return sb.ToString();
    }

    public string[] ReadStrings(ReadOnlySpan<string> separators,
                                ReadOnlySpan<string> terminators)
      => this.ReadSplitUpToAndPastTerminatorsIncludingEmpty_(
                 separators,
                 terminators)
             .ToArray();

    public string ReadLine()
      => this.ReadUpToAndPastTerminator(TextReaderConstants.NEWLINE_STRINGS);
  }
}