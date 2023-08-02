using System.Runtime.CompilerServices;

using CommunityToolkit.HighPerformance;

namespace System.IO {
  public interface ISpanElementReverser {
    void Reverse(Span<byte> span);
    void ReverseElements(Span<byte> span, int stride);
  }

  public class SpanElementReverser : ISpanElementReverser {
    public void Reverse(Span<byte> span) => span.Reverse();

    public void ReverseElements(Span<byte> span, int stride) {
      for (var i = 0; i < span.Length; i += stride) {
        span.Slice(i, stride).Reverse();
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

    public Stream BaseStream {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get;
      set;
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
    public void FillBuffer<T>(Span<T> buffer) where T : unmanaged
      => this.BaseStream.Read(buffer.AsBytes());


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void FillBufferAndReverse<T>(Span<T> buffer)
        where T : unmanaged {
      var bSpan = buffer.AsBytes();
      this.BaseStream.Read(bSpan);

      var sizeOf = sizeof(T);
      this.reverserImpl_.ReverseElements(bSpan, sizeOf);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FillBuffer(Span<byte> buffer, int? optStride = null) {
      var stride = optStride ?? buffer.Length;
      this.BaseStream.Read(buffer);
      this.reverserImpl_.ReverseElements(buffer, stride);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Read<T>() where T : unmanaged {
      Read(out T val);
      return val;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void Read<T>(out T val) where T : unmanaged {
      var size = sizeof(T);

      fixed (T* ptr = &val) {
        var bSpan = new Span<byte>(ptr, size);
        this.BaseStream.Read(bSpan);
        this.reverserImpl_.Reverse(bSpan);
      }
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
    public void PushStructureEndianness(Endianness endianness) {
      this.endiannessImpl_.PushStructureEndianness(endianness);
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
          newOppositeEndiannessOfSystem && this.reverserImpl_ != null) {
        return;
      }

      this.isCurrentlyOppositeEndianness_ = newOppositeEndiannessOfSystem;
      this.reverserImpl_ = newOppositeEndiannessOfSystem
          ? new SpanElementReverser()
          : new NoopSpanElementReverser();
    }
  }
}