using System.IO;

namespace schema.text {
  internal static class TextSchemaTestUtil {
    public static FinTextReader CreateTextReader(string text) {
      var ms = new MemoryStream();

      var sw = new StreamWriter(ms);
      sw.Write(text);
      sw.Flush();
      ms.Position = 0;

      return new FinTextReader(ms);
    }
  }
}