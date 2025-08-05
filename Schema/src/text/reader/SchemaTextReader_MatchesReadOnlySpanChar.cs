using System;
using System.Text;


namespace schema.text.reader;

public sealed partial class SchemaTextReader {
  public bool Matches(out char match, ReadOnlySpan<char> matches) {
    foreach (var c in matches) {
      if (this.PeekCharAndProgressIfEqualTo_(c)) {
        match = c;
        return true;
      }
    }

    match = default;
    return false;
  }

  public string ReadUpToStartOfTerminator(ReadOnlySpan<char> terminators) {
    var sb = new StringBuilder();

    while (!this.Eof) {
      var originalLineNumber = this.LineNumber;
      var originalIndexInLine = this.IndexInLine;
      var originalPosition = this.PositionInternal_;

      if (this.Matches(out _, terminators)) {
        this.LineNumber = originalLineNumber;
        this.IndexInLine = originalIndexInLine;
        this.PositionInternal_ = originalPosition;
        break;
      }

      sb.Append(this.ReadChar());
    }

    return sb.ToString();
  }

  public string ReadUpToAndPastTerminator(ReadOnlySpan<char> terminators) {
    var sb = new StringBuilder();

    while (!this.Eof) {
      if (this.Matches(out _, terminators)) {
        break;
      }

      sb.Append(this.ReadChar());
    }

    return sb.ToString();
  }

  public string ReadWhile(ReadOnlySpan<char> matches) {
    var sb = new StringBuilder();

    while (!this.Eof && this.Matches(out var c, matches)) {
      sb.Append(c);
    }

    return sb.ToString();
  }

  public void SkipManyIfPresent(ReadOnlySpan<char> matches) {
    while (!this.Eof && this.Matches(out _, matches)) { }
  }

  public void SkipOnceIfPresent(ReadOnlySpan<char> matches)
    => this.Matches(out _, matches);
}