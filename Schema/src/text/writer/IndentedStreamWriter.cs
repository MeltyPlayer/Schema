using System;
using System.IO;
using System.Runtime.CompilerServices;


namespace schema.text.writer;

public interface IIndentable {
  int CurrentIndentAmount { get; }
  void EnterBlockAndIndent();
  void ExitBlockAndOutdent();
}

public interface IIndentedStreamWriter : ITextStream, IIndentable {
  void WriteChar(char c);
  void WriteChars(ReadOnlySpan<char> chars);
  void WriteChars(ReadOnlySpan<char> chars, string separator);

  void WriteString(string s);
  void WriteStrings(ReadOnlySpan<string> strings, string separator);
}

// TODO: Support other than just ASCII
public class IndentedStreamWriter : IIndentedStreamWriter {
  private readonly Stream impl_;

  public IndentedStreamWriter(Stream impl, int tabWidth) {
      this.impl_ = impl;
      this.TabWidth = tabWidth;
    }

  public int TabWidth { get; }
  public const bool CONVERT_TABS_TO_SPACES = false;

  public int LineNumber { get; private set; }
  public int IndexInLine { get; private set; }
  public int CurrentIndentAmount { get; private set; } = 0;

  public void EnterBlockAndIndent() {
      this.CurrentIndentAmount++;
      this.WriteChar('\n');
    }

  public void ExitBlockAndOutdent() {
      this.CurrentIndentAmount--;
      this.WriteChar('\n');
    }

  public void WriteChar(char c) {
      if (c == '\n') {
        ++this.LineNumber;
        this.IndexInLine = this.TabWidth * this.CurrentIndentAmount;
        if (CONVERT_TABS_TO_SPACES) {
          for (var i = 0; i < this.IndexInLine; ++i) {
            this.impl_.WriteByte((byte) ' ');
          }
        } else {
          for (var i = 0; i < this.CurrentIndentAmount; ++i) {
            this.impl_.WriteByte((byte) '\t');
          }
        }
      } else if (c == '\t') {
        var remainingTabAmount = this.IndexInLine % this.TabWidth;
        this.IndexInLine += remainingTabAmount;

        if (CONVERT_TABS_TO_SPACES) {
          for (var i = 0; i < remainingTabAmount; ++i) {
            this.impl_.WriteByte((byte) ' ');
          }
        } else {
          this.impl_.WriteByte((byte) '\t');
        }
      } else {
        if (!char.IsControl(c)) {
          ++this.IndexInLine;
        }

        this.impl_.WriteByte((byte) c);
      }
    }

  public void WriteChars(ReadOnlySpan<char> chars) {
      foreach (var c in chars) {
        this.WriteChar(c);
      }
    }

  public void WriteChars(ReadOnlySpan<char> chars, string separator) {
      foreach (var c in chars) {
        this.WriteChar(c);
        this.WriteString(separator);
      }
    }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void WriteString(string s) => this.WriteChars(s.AsSpan());

  public void WriteStrings(ReadOnlySpan<string> strings, string separator) {
      foreach (var s in strings) {
        this.WriteString(s);
        this.WriteString(separator);
      }
    }
}