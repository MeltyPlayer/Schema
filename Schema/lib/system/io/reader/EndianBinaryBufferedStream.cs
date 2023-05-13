using System.Runtime.CompilerServices;

using CommunityToolkit.HighPerformance;

namespace System.IO {
  public class EndianBinaryBufferedStream : IEndiannessStack {
    private readonly EndiannessStackImpl endiannessImpl_;

    public EndianBinaryBufferedStream(Endianness? endianness) {
      this.endiannessImpl_ = new EndiannessStackImpl(endianness);
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
    public unsafe void FillBuffer<T>(T[] buffer, int count)
        where T : unmanaged {
      fixed (T* tPtr = buffer) {
        this.BaseStream.Read(new Span<byte>(tPtr, sizeof(T) * count));
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void FillBufferAndReverse<T>(T[] buffer, int count)
        where T : unmanaged
      => FillBufferAndReverse(buffer, count, sizeof(T));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void FillBufferAndReverse<T>(T[] buffer, int count, int sizeOf)
    where T : unmanaged {
      fixed (T* tPtr = buffer) {
        this.BaseStream.Read(new Span<byte>(tPtr, sizeOf * count));

        if (sizeOf == 1 || !this.IsOppositeEndiannessOfSystem) {
          return;
        }

        for (var i = 0; i < count; ++i) {
          new Span<byte>(tPtr + i, sizeOf).Reverse();
        }
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FillBuffer(Span<byte> buffer, int? optStride = null) {
      var stride = optStride ?? buffer.Length;
      this.BaseStream.Read(buffer);

      if (stride == 1 || !this.IsOppositeEndiannessOfSystem) {
        return;
      }

      for (var i = 0L; i < buffer.Length; i += stride) {
        buffer.Slice((int) i, (int) stride).Reverse();
      }
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

        if (size > 1 && this.IsOppositeEndiannessOfSystem) {
          bSpan.Reverse();
        }
      }
    }



    public Endianness Endianness => this.endiannessImpl_.Endianness;

    public bool IsOppositeEndiannessOfSystem {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this.endiannessImpl_.IsOppositeEndiannessOfSystem;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushStructureEndianness(Endianness endianness)
      => this.endiannessImpl_.PushStructureEndianness(endianness);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushMemberEndianness(Endianness endianness)
      => this.endiannessImpl_.PushMemberEndianness(endianness);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PopEndianness() => this.endiannessImpl_.PopEndianness();
  }
}