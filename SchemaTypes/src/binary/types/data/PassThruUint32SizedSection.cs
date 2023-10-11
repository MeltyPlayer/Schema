using schema.binary.attributes;

namespace schema.binary.types.data {
  [BinarySchema]
  public partial class PassThruUInt32SizedSection<T> : ISizedSection<T>
      where T : IBinaryConvertible {
    [Ignore]
    private readonly int tweakSize_;

    [WSizeOfMemberInBytes(nameof(Data))]
    private uint size_;

    [Ignore]
    public uint Size => this.size_;

    public T Data { get; }

    public PassThruUInt32SizedSection(T data) {
      this.Data = data;
    }

    public PassThruUInt32SizedSection(T data, int tweakSize) {
      this.Data = data;
      this.tweakSize_ = tweakSize;
    }

    public void Read(IEndianBinaryReader er) {
      this.size_ = er.ReadUInt32();

      var useSize = this.size_ + this.tweakSize_;
      var basePosition = er.Position;
      er.SubreadAt(er.Position, (int) useSize, this.Data.Read);
      er.Position = basePosition + useSize;
    }
  }
}