using System;
using System.IO;
using System.Runtime.CompilerServices;

using CommunityToolkit.HighPerformance;

namespace schema.util.streams {
  public class WritableStream(Stream impl) : ISeekableWritableStream {
    public static implicit operator WritableStream(Stream impl) => new(impl);

    internal Stream Impl => impl;
    public void Dispose() => impl.Dispose();

    public long Position {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => impl.Position;
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set => impl.Position = value;
    }

    public long Length {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => impl.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteByte(byte b) => impl.WriteByte(b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ReadOnlySpan<byte> src) => impl.Write(src);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(IReadableStream readableStream)
      => readableStream.CopyTo(impl);
  }
}