using System.Linq;


namespace schema.text.reader;

public sealed class TextReaderConstants {
  public static readonly string[] COMMA_STRINGS = [","];

  public static readonly string[] NEWLINE_STRINGS = ["\n", "\r\n"];

  public static readonly string[] WHITESPACE_STRINGS =
      TextReaderConstants.NEWLINE_STRINGS.Concat([" ", "\t"])
                         .ToArray();

  public static readonly string[] TERMINATORS =
      TextReaderConstants.WHITESPACE_STRINGS.Concat(COMMA_STRINGS).ToArray();
}