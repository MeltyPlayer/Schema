using System;
using System.Text;


namespace schema.text.reader;

public sealed partial class SchemaTextReader {
  public bool Matches(string match) {
    if (this.Eof) {
      return false;
    }

    var originalLineNumber = this.LineNumber;
    var originalIndexInLine = this.IndexInLine;
    var originalPosition = this.PositionInternal_;

    var maxLength = match.Length;

    var readLength =
        Math.Min(maxLength, this.Length - this.PositionInternal_);
    Span<char> peeked = stackalloc char[(int) readLength];
    this.ReadChars(peeked);

    this.LineNumber = originalLineNumber;
    this.IndexInLine = originalIndexInLine;
    this.PositionInternal_ = originalPosition;

    for (var j = 0; j < match.Length; ++j) {
      if (match[j] != peeked[j]) {
        return false;
      }
    }

    this.Position += match.Length;
    return true;
  }

  public string ReadUpToStartOfTerminator(string terminator) {
    var sb = new StringBuilder();

    while (!this.Eof) {
      var originalLineNumber = this.LineNumber;
      var originalIndexInLine = this.IndexInLine;
      var originalPosition = this.PositionInternal_;

      if (this.Matches(terminator)) {
        this.LineNumber = originalLineNumber;
        this.IndexInLine = originalIndexInLine;
        this.PositionInternal_ = originalPosition;
        break;
      }

      sb.Append(this.ReadChar());
    }

    return sb.ToString();
  }

  public string ReadUpToAndPastTerminator(string terminator) {
    var sb = new StringBuilder();

    while (!this.Eof) {
      if (this.Matches(terminator)) {
        break;
      }

      sb.Append(this.ReadChar());
    }

    return sb.ToString();
  }

  public void SkipUpToStartOfTerminator(string terminator) {
    while (!this.Eof) {
      var originalLineNumber = this.LineNumber;
      var originalIndexInLine = this.IndexInLine;
      var originalPosition = this.PositionInternal_;

      if (this.Matches(terminator)) {
        this.LineNumber = originalLineNumber;
        this.IndexInLine = originalIndexInLine;
        this.PositionInternal_ = originalPosition;
        break;
      }

      this.ReadChar();
    }
  }

  public void SkipUpToAndPastTerminator(string terminator) {
    while (!this.Eof) {
      if (this.Matches(terminator)) {
        break;
      }

      this.ReadChar();
    }
  }

  public string ReadWhile(string match) {
    var sb = new StringBuilder();

    while (!this.Eof && this.Matches(match)) {
      sb.Append(match);
    }

    return sb.ToString();
  }

  public void SkipManyIfPresent(string match) {
    while (!this.Eof && this.Matches(match)) { }
  }

  public void SkipOnceIfPresent(string match) => this.Matches(match);
}