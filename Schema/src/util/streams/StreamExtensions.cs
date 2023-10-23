using System.Runtime.CompilerServices;

namespace schema.util.streams {
  public static class StreamExtensions {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] ReadAllBytes(this ISizedReadableStream stream) {
      var bytes = new byte[stream.Length - stream.Position];
      stream.Read(bytes);
      return bytes;
    }
  }
}