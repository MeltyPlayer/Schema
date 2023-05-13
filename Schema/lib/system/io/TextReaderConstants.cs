using System.Linq;

namespace System.IO {
  public sealed class TextReaderConstants {
    public static readonly string[] COMMA_STRINGS = { "," };

    public static readonly string[] NEWLINE_STRINGS = { "\n", "\r\n" };

    public static readonly string[] WHITESPACE_STRINGS =
        TextReaderConstants.NEWLINE_STRINGS.Concat(new[] { " ", "\t" })
                           .ToArray();
  }
}