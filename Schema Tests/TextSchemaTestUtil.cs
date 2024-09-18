using System.IO;

using schema.text.reader;


namespace schema.text;

internal static class TextSchemaTestUtil {
  public static SchemaTextReader CreateTextReader(string text) {
    var ms = new MemoryStream();

    var sw = new StreamWriter(ms);
    sw.Write(text);
    sw.Flush();
    ms.Position = 0;

    return new SchemaTextReader(ms);
  }
}