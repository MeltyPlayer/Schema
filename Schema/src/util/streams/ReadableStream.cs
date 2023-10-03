using System;
using System.IO;

using CommunityToolkit.HighPerformance;

namespace schema.util.streams {
  public class ReadableStream : ISeekableReadableStream {
    private readonly Stream impl_;

    public ReadableStream(Stream impl) {
      if (!impl.CanRead) {
        throw new ArgumentException(nameof(impl));
      }

      this.impl_ = impl;
    }

    public ReadableStream(byte[] impl) : this(new MemoryStream(impl)) { }

    ~ReadableStream() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() => this.impl_.Dispose();

    public long Position {
      get => this.impl_.Position;
      set => this.impl_.Position = value;
    }

    public long Length => this.impl_.Length;

    public int Read(Span<byte> dst) => this.impl_.Read(dst);
  }
}