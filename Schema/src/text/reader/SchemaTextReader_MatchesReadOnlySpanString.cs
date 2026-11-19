using System;
using System.Text;

using schema.util.strings;


namespace schema.text.reader;

public sealed partial class SchemaTextReader {
  public bool Matches(out string text, ReadOnlySpan<string> matches) {
    if (this.Eof) {
      text = default;
      return false;
    }

    var originalLineNumber = this.LineNumber;
    var originalIndexInLine = this.IndexInLine;
    var originalPosition = this.PositionInternal_;

    var maxLength = 0;
    var minLength = int.MaxValue;
    for (var i = 0; i < matches.Length; ++i) {
      var matchLength = matches[i].Length;
      maxLength = Math.Max(maxLength, matchLength);
      minLength = Math.Min(minLength, matchLength);
    }

    var readLength = Math.Min(maxLength, this.impl_.MaxRemainingChars);
    if (readLength < minLength) {
      text = default;
      return false;
    }

    Span<char> peeked = stackalloc char[readLength];
    var charCount = this.impl_.ReadChars(peeked);
    peeked = peeked[..charCount];

    this.LineNumber = originalLineNumber;
    this.IndexInLine = originalIndexInLine;
    this.PositionInternal_ = originalPosition;

    for (var i = 0; i < matches.Length; ++i) {
      var match = matches[i];
      if (match.Length > peeked.Length) {
        continue;
      }

      for (var j = 0; j < match.Length; ++j) {
        if (match[j] != peeked[j]) {
          goto DidNotMatch;
        }
      }

      this.Position += this.impl_.Encoding.GetByteCount(match);
      text = match;
      return true;

      DidNotMatch: ;
    }

    text = string.Empty;
    return false;
  }

  public string ReadUpToStartOfTerminator(ReadOnlySpan<string> terminators) {
    using var sb = new ValueStringBuilder();

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
    using var sb = new ValueStringBuilder();

    while (!this.Eof) {
      if (this.Matches(out _, terminators)) {
        break;
      }

      sb.Append(this.ReadChar());
    }

    return sb.ToString();
  }

  public string ReadWhile(ReadOnlySpan<string> matches) {
    using var sb = new ValueStringBuilder();

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