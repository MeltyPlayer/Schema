using System;
using System.IO;

namespace schema.io {
  internal static class StreamReaderExtensions {
    public static int Read(this StreamReader sr,
                           char[] buffer,
                           int start,
                           int length)
      => sr.Read(buffer.AsSpan(start, length));

    public static int Read(this StreamReader sr, Span<char> buffer) {
      var readCount = 0;
      for (var i = 0; i < buffer.Length; i++) {
        var c = sr.Read();
        if (c == -1) {
          break;
        }

        buffer[i] = (char) c;
        readCount++;
      }

      return readCount;
    }
  }
}
