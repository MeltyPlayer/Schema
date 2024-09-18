using System;
using System.IO;
using System.Runtime.CompilerServices;

using CommunityToolkit.HighPerformance;


namespace schema.util.streams;

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Impl.Position;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set => this.Impl.Position = value;
  }

  public long Length {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Impl.Length;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public byte ReadByte() => (byte) this.Impl.ReadByte();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int TryToReadIntoBuffer(Span<byte> dst) => this.Impl.Read(dst);
}