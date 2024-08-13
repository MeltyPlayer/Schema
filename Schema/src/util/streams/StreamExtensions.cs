using System;
using System.IO;
using System.Runtime.CompilerServices;

using CommunityToolkit.HighPerformance;


namespace schema.util.streams;

public static class StreamExtensions {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte[] ReadAllBytes(this ISizedReadableStream stream) {
      var bytes = new byte[stream.Length - stream.Position];
      stream.TryToReadIntoBuffer(bytes);
      return bytes;
    }

  /// <summary>
  ///   (Straight-up copied from the implementation of Stream.CopyTo())
  ///   We pick a value that is the largest multiple of 4096 that is still smaller than the large object heap threshold (85K).
  ///   The CopyTo/CopyToAsync buffer is short-lived and is likely to be collected at Gen0, and it offers a significant
  ///   improvement in Copy performance.
  /// </summary>
  private const int DEFAULT_COPY_BUFFER_SIZE = 81920;

  public static void CopyTo(this IReadableStream me, Stream other) {
      if (me is ReadableStream readableStreamImpl) {
        readableStreamImpl.Impl.CopyTo(other);
        return;
      }

      Span<byte> buffer = stackalloc byte[DEFAULT_COPY_BUFFER_SIZE];
      int bytesRead;
      while ((bytesRead = me.TryToReadIntoBuffer(buffer)) != 0) {
        other.Write(buffer.Slice(0, bytesRead));
      }
    }

  public static void CopyTo(this IReadableStream me, IWritableStream other) {
      if (me is ReadableStream readableStreamImpl &&
          other is WritableStream writableStreamImpl) {
        readableStreamImpl.CopyTo(writableStreamImpl.Impl);
        return;
      }

      Span<byte> buffer = stackalloc byte[DEFAULT_COPY_BUFFER_SIZE];
      int bytesRead;
      while ((bytesRead = me.TryToReadIntoBuffer(buffer)) != 0) {
        other.Write(buffer.Slice(0, bytesRead));
      }
    }

  public static void CopyTo(this Stream me, IWritableStream other) {
      if (other is WritableStream writableStreamImpl) {
        me.CopyTo(writableStreamImpl.Impl);
        return;
      }

      Span<byte> buffer = stackalloc byte[DEFAULT_COPY_BUFFER_SIZE];
      int bytesRead;
      while ((bytesRead = me.Read(buffer)) != 0) {
        other.Write(buffer.Slice(0, bytesRead));
      }
    }
}