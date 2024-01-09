using System;
using System.Text;

namespace schema.text.reader {
  public sealed partial class SchemaTextReader {
    public bool Matches(out string text, ReadOnlySpan<string> matches) {
      var originalLineNumber = this.LineNumber;
      var originalIndexInLine = this.IndexInLine;
      var originalPosition = this.PositionInternal_;

      var maxLength = 0;
      for (var i = 0; i < matches.Length; ++i) {
        maxLength = Math.Max(maxLength, matches[i].Length);
      }

      var readLength =
          Math.Min(maxLength, this.Length - this.PositionInternal_);
      Span<char> peeked = stackalloc char[(int) readLength];
      this.ReadChars(peeked);

      this.LineNumber = originalLineNumber;
      this.IndexInLine = originalIndexInLine;
      this.PositionInternal_ = originalPosition;

      for (var i = 0; i < matches.Length; ++i) {
        var match = matches[i];
        for (var j = 0; j < match.Length; ++j) {
          if (match[j] != peeked[j]) {
            goto DidNotMatch;
          }
        }

        this.Position += match.Length;
        text = match;
        return true;

        DidNotMatch: ;
      }

      text = string.Empty;
      return false;
    }

    public string ReadUpToStartOfTerminator(ReadOnlySpan<string> terminators) {
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

    public string ReadUpToAndPastTerminator(ReadOnlySpan<string> terminators) {
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

    public string ReadWhile(ReadOnlySpan<string> matches) {
      var sb = new StringBuilder();

      while (!this.Eof && this.Matches(out var text, matches)) {
        sb.Append(text);
      }

      return sb.ToString();
    }

    public void SkipManyIfPresent(ReadOnlySpan<string> matches) {
      while (!this.Eof && this.Matches(out _, matches)) { }
    }

    public void SkipOnceIfPresent(ReadOnlySpan<string> matches)
      => this.Matches(out _, matches);
  }
}