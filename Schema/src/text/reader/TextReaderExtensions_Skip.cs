namespace schema.text.reader;

public static partial class TextReaderExtensions {
  public static void SkipWhitespace(this ITextReader tr)
    => tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);

  public static void SkipToEndOfLine(this ITextReader tr) {
    if (tr.Eof) {
      return;
    }

    char c;
    do {
      c = tr.ReadChar();
    } while (c is not ('\n' or '\r'));

    if (c == '\r') {
      tr.SkipOnceIfPresent('\n');
    }
  }

  public static void SkipComments(
      this ITextReader tr,
      string lineCommentPrefix = "//") {
    TryAgain:

    tr.SkipWhitespace();

    if (tr.Matches(lineCommentPrefix)) {
      tr.SkipToEndOfLine();
      goto TryAgain;
    }

    if (tr.Matches("/*")) {
      tr.ReadUpToAndPastTerminator("*/");
      goto TryAgain;
    }
  }
}