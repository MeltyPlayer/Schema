using System.Runtime.CompilerServices;

namespace System.IO {
  public sealed partial class EndianBinaryWriter : IEndiannessStack {
    public Endianness Endianness {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this.impl_.Endianness;
    }

    public bool IsOppositeEndiannessOfSystem {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this.impl_.IsOppositeEndiannessOfSystem;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushStructureEndianness(Endianness endianness)
      => this.impl_.PushStructureEndianness(endianness);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushMemberEndianness(Endianness endianness)
      => this.impl_.PushMemberEndianness(endianness);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PopEndianness() => this.impl_.PopEndianness();
  }
}