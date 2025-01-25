using System.Linq;


namespace schema.text.reader;

public sealed class TextReaderConstants {
  public static readonly char COMMA_CHAR = ',';
  public static readonly char[] COMMA_CHARS = [COMMA_CHAR];

  public static readonly char[] NEWLINE_CHARS = ['\n', '\r'];

  public static readonly string[] NEWLINE_STRINGS
      = NEWLINE_CHARS.Select(c => c.ToString()).ToArray();

  public static readonly char[] WHITESPACE_CHARS =
      TextReaderConstants.NEWLINE_CHARS.Concat([' ', '\t']).ToArray();

  public static readonly string[] WHITESPACE_STRINGS
      = WHITESPACE_CHARS.Select(c => c.ToString()).ToArray();

  public static readonly char[] TERMINATORS =
      TextReaderConstants.WHITESPACE_CHARS.Concat(COMMA_CHARS).ToArray();
}