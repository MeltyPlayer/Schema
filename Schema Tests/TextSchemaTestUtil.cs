using System.IO;

using schema.text.reader;

using TextReader = schema.text.reader.TextReader;

namespace schema.text {
  internal static class TextSchemaTestUtil {
    public static TextReader CreateTextReader(string text) {
      var ms = new MemoryStream();

      var sw = new StreamWriter(ms);
      sw.Write(text);
      sw.Flush();
      ms.Position = 0;

      return new TextReader(ms);
    }
  }
}