using schema.binary.attributes;


namespace schema.binary.types.data;

[BinarySchema]
public partial class PassThruUInt32SizedSection<T> : ISizedSection<T>
    where T : IBinaryConvertible {
  [Skip]
  private readonly int tweakSize_;

  [WSizeOfMemberInBytes(nameof(Data))]
  private uint size_;

  [Skip]
  public uint Size => this.size_;

  public T Data { get; }

  public PassThruUInt32SizedSection(T data) {
    this.Data = data;
  }

  public PassThruUInt32SizedSection(T data, int tweakSize) {
    this.Data = data;
    this.tweakSize_ = tweakSize;
  }

  public void Read(IBinaryReader br) {
    this.size_ = br.ReadUInt32();

    var useSize = this.size_ + this.tweakSize_;
    var basePosition = br.Position;
    br.SubreadAt(br.Position, (int) useSize, () => this.Data.Read(br));
    br.Position = basePosition + useSize;
  }
}