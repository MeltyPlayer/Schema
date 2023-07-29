namespace System.IO {
  public sealed partial class EndianBinaryWriter : IEndiannessStack {
    public Endianness Endianness => this.impl_.Endianness;

    public bool IsOppositeEndiannessOfSystem
      => this.impl_.IsOppositeEndiannessOfSystem;

    public void PushStructureEndianness(Endianness endianness)
      => this.impl_.PushStructureEndianness(endianness);

    public void PushMemberEndianness(Endianness endianness)
      => this.impl_.PushMemberEndianness(endianness);

    public void PopEndianness() => this.impl_.PopEndianness();
  }
}