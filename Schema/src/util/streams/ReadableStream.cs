using System;
using System.IO;

using CommunityToolkit.HighPerformance;

namespace schema.util.streams {
  public class ReadableStream : ISeekableReadableStream {
    internal Stream Impl { get; }

    public static implicit operator ReadableStream(Stream impl) => new(impl);

    public ReadableStream(Stream impl) {
      if (!impl.CanRead) {
        throw new ArgumentException(nameof(impl));
      }

      this.Impl = impl;
    }

    public ReadableStream(byte[] impl) : this(new MemoryStream(impl)) { }

    public void Dispose() => this.Impl.Dispose();

    public long Position {
      get => this.Impl.Position;
      set => this.Impl.Position = value;
    }

    public long Length => this.Impl.Length;

    public int Read(Span<byte> dst) => this.Impl.Read(dst);
  }
}