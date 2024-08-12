using System;


namespace schema.text.reader {
  public sealed partial class SchemaTextReader {
    public bool Matches(out char match, ReadOnlySpan<char> matches) {
      var c = this.PeekChar_();

      for (var i = 0; i < matches.Length; ++i) {
        if (matches[i] == c) {
          match = c;
          this.ReadChar();
          return true;
        }
      }

      match = default;
      return false;
    }
  }
}