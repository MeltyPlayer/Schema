namespace System.IO {
  public sealed partial class EndianBinaryWriter : IEndiannessStack {
    private readonly IEndiannessStack endiannessImpl_;

    public Endianness Endianness => this.endiannessImpl_.Endianness;

    public bool IsOppositeEndiannessOfSystem
      => this.endiannessImpl_.IsOppositeEndiannessOfSystem;

    public void PushStructureEndianness(Endianness endianness)
      => this.endiannessImpl_.PushStructureEndianness(endianness);

    public void PushMemberEndianness(Endianness endianness)
      => this.endiannessImpl_.PushMemberEndianness(endianness);

    public void PopEndianness() => this.endiannessImpl_.PopEndianness();
  }
}