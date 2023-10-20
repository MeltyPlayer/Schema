using System.Runtime.CompilerServices;

using schema.src.util;

namespace schema.util.streams {
  public static class StreamExtensions {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ReadByte(this IReadableStream stream) {
      byte value = default;
      stream.Read(UnsafeUtil.AsSpan(ref value));
      return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] ReadAllBytes(this ISizedReadableStream stream) {
      var bytes = new byte[stream.Length - stream.Position];
      stream.Read(bytes);
      return bytes;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteByte(this IWritableStream stream, byte value)
      => stream.Write(UnsafeUtil.AsSpan(ref value));
  }
}