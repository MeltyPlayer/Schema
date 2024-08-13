namespace schema.util.text;

public static class SourceWriterExtensions {
  public static ISourceWriter EnterBlock(this ISourceWriter sw,
                                         string prefix = "") {
    if (prefix.Length > 0) {
      prefix = $"{prefix} ";
    }

    return sw.WriteLine($"{prefix}{{");
  }

  public static ISourceWriter ExitBlock(this ISourceWriter sw)
    => sw.WriteLine("}");

  public static ISourceWriter WriteLine(this ISourceWriter sw, string text)
    => sw.Write(text + '\n');
}