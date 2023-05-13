namespace System.IO {
  public sealed partial class EndianBinaryReader {
    public Endianness Endianness => this.BufferedStream_.Endianness;

    public bool IsOppositeEndiannessOfSystem
      => this.BufferedStream_.IsOppositeEndiannessOfSystem;

    public void PushStructureEndianness(Endianness endianness)
      => this.BufferedStream_.PushStructureEndianness(endianness);

    public void PushMemberEndianness(Endianness endianness)
      => this.BufferedStream_.PushMemberEndianness(endianness);

    public void PopEndianness() => this.BufferedStream_.PopEndianness();
  }
}