using System.IO;
using System.Text;

namespace schema.util.streams;

public static class StreamReaderUtil {
  public static UnbufferedStreamReader FromString(string text) {
    var ms = new MemoryStream();

    var sw = new StreamWriter(ms, Encoding.Unicode);
    sw.Write(text);
    sw.Flush();
    ms.Position = 0;

    return new UnbufferedStreamReader(ms, Encoding.Unicode);
  }
}