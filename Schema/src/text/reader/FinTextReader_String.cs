using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using CommunityToolkit.HighPerformance;

using schema.util;

namespace schema.text.reader {
  public sealed partial class FinTextReader {
    public void AssertChar(char expectedValue)
      => Asserts.Equal(expectedValue, this.ReadChar());

    // TODO: Handle other encodings besides ASCII
    public char ReadChar() {
      var c = (char) this.baseStream_.ReadByte();
      this.IncrementLineIndicesForChar_(c);
      return c;
    }

    public char[] ReadChars(long count) {
      var newArray = new char[count];
      this.ReadChars(newArray);
      return newArray;
    }

    public void ReadChars(char[] dst, int start, int length)
      => this.ReadChars(dst.AsSpan(start, length));

    // TODO: Handle other encodings besides ASCII
    public void ReadChars(Span<char> dst) {
      this.baseStream_.Read(dst.AsBytes());

      foreach (var c in dst) {
        this.IncrementLineIndicesForChar_(c);
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

    public string[] ReadStrings(string[] separators, string[] terminators)
      => this.ReadSplitUpToAndPastTerminatorsIncludingEmpty_(separators, terminators)
             .ToArray();

    public string ReadLine()
      => this.ReadUpToAndPastTerminator(TextReaderConstants.NEWLINE_STRINGS);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void IncrementLineIndicesForChar_(char c) {
      if (c == '\n') {
        this.IndexInLine = 0;
        ++this.LineNumber;
      } else if (c == '\t') {
        this.IndexInLine += this.TabWidth;
      } else if (!char.IsControl(c)) {
        ++this.IndexInLine;
      }
    }
  }
}