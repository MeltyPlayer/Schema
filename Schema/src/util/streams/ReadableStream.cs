using System;
using System.IO;
using System.Runtime.CompilerServices;

using CommunityToolkit.HighPerformance;

using schema.util.asserts;


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
  public byte ReadByte() {
    var value = this.Impl.ReadByte();
    Asserts.False(value == -1);
    return (byte) value;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ReadIntoBuffer(Span<byte> dst)
    => Asserts.Equal(dst.Length, this.TryToReadIntoBuffer(dst));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int TryToReadIntoBuffer(Span<byte> dst) => this.Impl.Read(dst);
}