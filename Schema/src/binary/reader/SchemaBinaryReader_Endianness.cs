using System.Runtime.CompilerServices;

namespace schema.binary {
  public sealed partial class SchemaBinaryReader {
    public Endianness Endianness {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this.bufferedStream_.Endianness;
    }

    public bool IsOppositeEndiannessOfSystem {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this.bufferedStream_.IsOppositeEndiannessOfSystem;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushContainerEndianness(Endianness endianness)
      => this.bufferedStream_.PushContainerEndianness(endianness);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushMemberEndianness(Endianness endianness)
      => this.bufferedStream_.PushMemberEndianness(endianness);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PopEndianness() => this.bufferedStream_.PopEndianness();
  }
}