using System.Runtime.CompilerServices;

namespace schema.binary {
  public sealed partial class EndianBinaryReader {
    public Endianness Endianness {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this.BufferedStream_.Endianness;
    }

    public bool IsOppositeEndiannessOfSystem {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this.BufferedStream_.IsOppositeEndiannessOfSystem;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushContainerEndianness(Endianness endianness)
      => this.BufferedStream_.PushContainerEndianness(endianness);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushMemberEndianness(Endianness endianness)
      => this.BufferedStream_.PushMemberEndianness(endianness);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PopEndianness() => this.BufferedStream_.PopEndianness();
  }
}