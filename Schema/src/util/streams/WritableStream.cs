using System;
using System.IO;

using CommunityToolkit.HighPerformance;

namespace schema.util.streams {
  public class WritableStream(Stream impl) : ISeekableWritableStream {
    public void Dispose() => impl.Dispose();

    public long Position {
      get => impl.Position;
      set => impl.Position = value;
    }

    public long Length => impl.Length;

    public void Write(ReadOnlySpan<byte> src) => impl.Write(src);
  }
}