using System;
using System.IO;

using CommunityToolkit.HighPerformance;

namespace schema.util.streams {
  public class WritableStream(Stream impl) : ISeekableWritableStream {
    /// <summary>
    ///   (Straight-up copied from the implementation of Stream.CopyTo())
    ///   We pick a value that is the largest multiple of 4096 that is still smaller than the large object heap threshold (85K).
    ///   The CopyTo/CopyToAsync buffer is short-lived and is likely to be collected at Gen0, and it offers a significant
    ///   improvement in Copy performance.
    /// </summary>
    private const int DEFAULT_COPY_BUFFER_SIZE = 81920;

    public static implicit operator WritableStream(Stream impl) => new(impl);

    public void Dispose() => impl.Dispose();

    public long Position {
      get => impl.Position;
      set => impl.Position = value;
    }

    public long Length => impl.Length;

    public void WriteByte(byte b) => impl.WriteByte(b);
    public void Write(ReadOnlySpan<byte> src) => impl.Write(src);

    public void Write(IReadableStream readableStream) {
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