using System;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;

using CommunityToolkit.HighPerformance;

using schema.src.util;
using schema.util.streams;

using Tedd;


namespace schema.binary;

public interface ISpanElementReverser {
  void Reverse(Span<byte> span);
  void ReverseElements(Span<byte> span, int stride);
}

public class SpanElementReverser : ISpanElementReverser {
  public void Reverse(Span<byte> span) => span.Reverse();

  public void ReverseElements(Span<byte> bytes, int stride) {
    if (stride == 2) {
      var shorts = bytes.Cast<byte, short>();
      for (var i = 0; i < shorts.Length; ++i) {
        shorts[i].ReverseEndianness();
      }

      return;
    }

    if (stride == 4) {
      var ints = bytes.Cast<byte, int>();
      for (var i = 0; i < ints.Length; ++i) {
        ints[i].ReverseEndianness();
      }

      return;
    }

    if (stride == 8) {
      var longs = bytes.Cast<byte, long>();
      for (var i = 0; i < longs.Length; ++i) {
        longs[i].ReverseEndianness();
      }

      return;
    }

    for (var i = 0; i < bytes.Length; i += stride) {
      bytes.Slice(i, stride).Reverse();
    }
  }
}

public class NoopSpanElementReverser : ISpanElementReverser {
  public void Reverse(Span<byte> span) { }
  public void ReverseElements(Span<byte> span, int stride) { }
}

public class EndianBinaryBufferedStream : IEndiannessStack {
  private readonly EndiannessStackImpl endiannessImpl_;

  private bool isCurrentlyOppositeEndianness_;
  private ISpanElementReverser reverserImpl_;

  public EndianBinaryBufferedStream(Endianness? endianness) {
    this.endiannessImpl_ = new EndiannessStackImpl(endianness);
    this.UpdateSpanElementReverser_();
  }

  public ISeekableReadableStream BaseStream {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get;
    set;
  }

  public bool Eof {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.BaseStream.Position >= this.BaseStream.Length;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AssertNotEof() {
    if (this.Eof) {
      throw new EndOfStreamException(
          $"Attempted to read past the end of the stream: position '{this.BaseStream.Position}' of stream length '{this.BaseStream.Length}'");
    }
  }

  public byte[] Buffer { get; private set; }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void FillBuffer(long count, int? optStride = null) {
    if (this.Buffer == null || this.Buffer.Length < count) {
      this.Buffer = new byte[count];
    }

    FillBuffer(new Span<byte>(this.Buffer, 0, (int) count), optStride);
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void FillBuffer<T>(Span<T> buffer) where T : unmanaged {
    if (buffer.Length == 0) {
      return;
    }

    this.BaseStream.ReadIntoBuffer(buffer.AsBytes());
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void FillBufferAndReverse<T>(Span<T> buffer)
      where T : unmanaged {
    if (buffer.Length == 0) {
      return;
    }

    var bSpan = buffer.AsBytes();
    this.BaseStream.ReadIntoBuffer(bSpan);

    var sizeOf = sizeof(T);
    this.reverserImpl_.ReverseElements(bSpan, sizeOf);
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void FillBuffer(Span<byte> buffer, int? optStride = null) {
    if (buffer.Length == 0) {
      return;
    }

    var stride = optStride ?? buffer.Length;
    this.BaseStream.ReadIntoBuffer(buffer);
    this.reverserImpl_.ReverseElements(buffer, stride);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public byte ReadByte() {
    return this.BaseStream.ReadByte();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public T Read<T>() where T : unmanaged {
    this.Read(out T val);
    return val;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Read<T>(out T val) where T : unmanaged {
    val = default;
    var bSpan = UnsafeUtil.AsSpan(ref val).AsBytes();
    this.BaseStream.ReadIntoBuffer(bSpan);
    this.reverserImpl_.Reverse(bSpan);
  }


  public Endianness Endianness {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.endiannessImpl_.Endianness;
  }

  public bool IsOppositeEndiannessOfSystem {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.endiannessImpl_.IsOppositeEndiannessOfSystem;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void PushContainerEndianness(Endianness endianness) {
    this.endiannessImpl_.PushContainerEndianness(endianness);
    this.UpdateSpanElementReverser_();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void PushMemberEndianness(Endianness endianness) {
    this.endiannessImpl_.PushMemberEndianness(endianness);
    this.UpdateSpanElementReverser_();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void PopEndianness() {
    this.endiannessImpl_.PopEndianness();
    this.UpdateSpanElementReverser_();
  }

  private void UpdateSpanElementReverser_() {
    var newOppositeEndiannessOfSystem =
        this.endiannessImpl_.IsOppositeEndiannessOfSystem;
    if (this.isCurrentlyOppositeEndianness_ ==
        newOppositeEndiannessOfSystem &&
        this.reverserImpl_ != null) {
      return;
    }

    this.isCurrentlyOppositeEndianness_ = newOppositeEndiannessOfSystem;
    this.reverserImpl_ = newOppositeEndiannessOfSystem
        ? new SpanElementReverser()
        : new NoopSpanElementReverser();
  }
}