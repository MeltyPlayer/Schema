using System;
using System.IO;
using System.Runtime.CompilerServices;

using CommunityToolkit.HighPerformance;

namespace schema.util.streams {
  public static class BasicStreamImpls {
    /// <summary>
    ///   (Straight-up copied from the implementation of Stream.CopyTo())
    ///   We pick a value that is the largest multiple of 4096 that is still smaller than the large object heap threshold (85K).
    ///   The CopyTo/CopyToAsync buffer is short-lived and is likely to be collected at Gen0, and it offers a significant
    ///   improvement in Copy performance.
    /// </summary>
    private const int DEFAULT_COPY_BUFFER_SIZE = 81920;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void WriteImpl(this Stream impl,
                                   IReadableStream readableStream) {
      if (readableStream is ReadableStream readableStreamImpl) {
        readableStreamImpl.Impl.CopyTo(impl);
        return;
      }

      Span<byte> buffer = stackalloc byte[DEFAULT_COPY_BUFFER_SIZE];
      int bytesRead;
      while ((bytesRead = readableStream.Read(buffer)) != 0) {
        impl.Write(buffer.Slice(0, bytesRead));
      }
    }
  }
}