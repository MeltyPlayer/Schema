using System.Runtime.CompilerServices;

namespace System.IO {
  public sealed partial class EndianBinaryReader {
    public Endianness Endianness => this.BufferedStream_.Endianness;

    public bool IsOppositeEndiannessOfSystem
      => this.BufferedStream_.IsOppositeEndiannessOfSystem;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushStructureEndianness(Endianness endianness)
      => this.BufferedStream_.PushStructureEndianness(endianness);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushMemberEndianness(Endianness endianness)
      => this.BufferedStream_.PushMemberEndianness(endianness);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PopEndianness() => this.BufferedStream_.PopEndianness();
  }
}