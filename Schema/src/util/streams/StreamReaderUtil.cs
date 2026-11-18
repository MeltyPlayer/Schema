using System.IO;
using System.Text;

namespace schema.util.streams;

public static class StreamReaderUtil {
  public static UnbufferedStreamReader FromString(string text, Encoding encoding) {
    var ms = new MemoryStream();

    var sw = new StreamWriter(ms, encoding);
    sw.Write(text);
    sw.Flush();
    ms.Position = 0;

    return new UnbufferedStreamReader(ms, encoding);
  }
}