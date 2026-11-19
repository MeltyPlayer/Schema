using System.Text;

using schema.util.strings;


namespace schema.text.reader;

public sealed partial class SchemaTextReader {
  public bool Matches(char match) => this.PeekCharAndProgressIfEqualTo_(match);

  public string ReadUpToStartOfTerminator(char terminator) {
    var sb = new StringBuilder();

    while (!this.Eof) {
      var originalLineNumber = this.LineNumber;
      var originalIndexInLine = this.IndexInLine;
      var originalPosition = this.PositionInternal_;

      if (this.PeekCharAndProgressIfNotEqualTo_(terminator, out var peeked)) {
        sb.Append(peeked);
      } else {
        this.LineNumber = originalLineNumber;
        this.IndexInLine = originalIndexInLine;
        this.PositionInternal_ = originalPosition;
        break;
      }
    }

    return sb.ToString();
  }

  public string ReadUpToAndPastTerminator(char terminator) {
    var sb = new ValueStringBuilder();
    this.ReadUpToAndPastTerminatorImpl_(ref sb, terminator);
    var value = sb.ToString();
    sb.Dispose();
    return value;
  }

  private void ReadUpToAndPastTerminatorImpl_(
      ref ValueStringBuilder sb,
      char terminator) {
    while (!this.Eof) {
      if (this.PeekCharAndProgressIfNotEqualTo_(terminator, out var peeked)) {
        sb.Append(peeked);
      } else {
        this.ReadChar();
        break;
      }
    }
  }

  public void SkipUpToStartOfTerminator(char terminator) {
    while (!this.Eof) {
      var originalLineNumber = this.LineNumber;
      var originalIndexInLine = this.IndexInLine;
      var originalPosition = this.PositionInternal_;

      if (this.PeekCharAndProgressIfNotEqualTo_(terminator, out _)) {
        continue;
      }

      this.LineNumber = originalLineNumber;
      this.IndexInLine = originalIndexInLine;
      this.PositionInternal_ = originalPosition;
      break;
    }
  }

  public void SkipUpToAndPastTerminator(char terminator) {
    while (!this.Eof) {
      if (this.PeekCharAndProgressIfNotEqualTo_(terminator, out _)) {
        continue;
      }

      this.ReadChar();
      break;
    }
  }

  public string ReadWhile(char matches) {
    var sb = new StringBuilder();

    while (!this.Eof && this.Matches(matches)) {
      sb.Append(matches);
    }

    return sb.ToString();
  }

  public void SkipManyIfPresent(char matches) {
    while (!this.Eof && this.Matches(matches)) { }
  }

  public void SkipOnceIfPresent(char matches) => this.Matches(matches);
}