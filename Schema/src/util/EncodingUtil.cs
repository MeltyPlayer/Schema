using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace schema.util {
  public static class EncodingUtil {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetEncodingSize(this Encoding encoding) {
      if (encoding == Encoding.ASCII || encoding == Encoding.UTF8) {
        return 1;
      }

      if (encoding == Encoding.Unicode ||
          encoding == Encoding.BigEndianUnicode) {
        return 2;
      }

      if (encoding == Encoding.UTF32) {
        return 4;
      }

      throw new NotImplementedException();
    }
  }
}
