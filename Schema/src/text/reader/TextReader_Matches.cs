using System;
using System.Text;

namespace schema.text.reader {
  public sealed partial class TextReader {
    public bool Matches(out string text, string[] matches) {
      var originalLineNumber = this.LineNumber;
      var originalIndexInLine = this.IndexInLine;
      var originalPosition = this.Position;

      foreach (var match in matches) {
        foreach (var c in match) {
          if (c != this.ReadChar()) {
            goto DidNotMatch;
          }
        }

        text = match;
        return true;

        DidNotMatch:
        this.LineNumber = originalLineNumber;
        this.IndexInLine = originalIndexInLine;
        this.Position = originalPosition;
      }

      text = String.Empty;
      return false;
    }

    public string ReadUpToStartOfTerminator(params string[] terminators) {
      var sb = new StringBuilder();

      while (!this.Eof) {
        var originalLineNumber = this.LineNumber;
        var originalIndexInLine = this.IndexInLine;
        var originalPosition = this.Position;

        if (this.Matches(out _, terminators)) {
          this.LineNumber = originalLineNumber;
          this.IndexInLine = originalIndexInLine;
          this.Position = originalPosition;
          break;
        }

        sb.Append(this.ReadChar());
      }

      return sb.ToString();
    }

    public string ReadUpToAndPastTerminator(params string[] terminators) {
      var sb = new StringBuilder();

      while (!this.Eof) {
        if (this.Matches(out _, terminators)) {
          break;
        }

        sb.Append(this.ReadChar());
      }

      return sb.ToString();
    }

    public string ReadWhile(params string[] matches) {
      var sb = new StringBuilder();

      while (!this.Eof && this.Matches(out var text, matches)) {
        sb.Append(text);
      }

      return sb.ToString();
    }

    public void IgnoreManyIfPresent(params string[] matches) {
      while (this.Matches(out _, matches)) { }
    }

    public void IgnoreOnceIfPresent(params string[] matches)
      => this.Matches(out _, matches);
  }
}