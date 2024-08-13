using System.Runtime.CompilerServices;


namespace schema.binary;

public sealed partial class SchemaBinaryWriter : IEndiannessStack {
  public Endianness Endianness {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.impl_.Endianness;
  }

  public bool IsOppositeEndiannessOfSystem {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.impl_.IsOppositeEndiannessOfSystem;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void PushContainerEndianness(Endianness endianness)
    => this.impl_.PushContainerEndianness(endianness);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void PushMemberEndianness(Endianness endianness)
    => this.impl_.PushMemberEndianness(endianness);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void PopEndianness() => this.impl_.PopEndianness();
}