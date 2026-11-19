using System;
using System.Runtime.CompilerServices;

using schema.util.strings;


namespace schema.text.reader;

public sealed partial class SchemaTextReader {
  public bool Matches(out char match, ReadOnlySpan<char> matches) {
    if (this.Eof) {
      match = default;
      return false;
    }

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

  public string ReadUpToAndPastTerminator(ReadOnlySpan<char> terminators) {
    using var sb = new ValueStringBuilder();

    while (!this.Eof) {
      if (this.Matches(out _, terminators)) {
        break;
      }

      sb.Append(this.ReadChar());
    }

    return sb.ToString();
  }

  public string ReadWhile(ReadOnlySpan<char> matches) {
    var sb = new ValueStringBuilder();
    this.ReadWhileImpl_(ref sb, matches);
    var str = sb.ToString();
    sb.Dispose();
    return str;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void ReadWhileImpl_(
      ref ValueStringBuilder sb,
      ReadOnlySpan<char> matches) {
    while (!this.Eof && this.Matches(out var c, matches)) {
      sb.Append(c);
    }
  }

  public void SkipManyIfPresent(ReadOnlySpan<char> matches) {
    while (!this.Eof && this.Matches(out _, matches)) { }
  }

  public void SkipOnceIfPresent(ReadOnlySpan<char> matches)
    => this.Matches(out _, matches);
}