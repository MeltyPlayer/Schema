using System.IO;

namespace schema.util.streams;

public static class StreamUtil {
  public static Stream FromString(string text) {
    var ms = new MemoryStream();

    var sw = new StreamWriter(ms);
    sw.Write(text);
    sw.Flush();
    ms.Position = 0;

    return ms;
  }
}