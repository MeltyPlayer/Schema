using schema.binary.attributes;

namespace schema.binary.types.data {
  [BinarySchema]
  public partial class PassThruMagicUInt32SizedSection<T> : IMagicSection<T>
      where T : IBinaryConvertible {
    public string Magic { get; }

    private readonly PassThruUInt32SizedSection<T> impl_;

    public PassThruMagicUInt32SizedSection(
        string magic,
        T data) {
      this.Magic = magic;
      this.impl_ = new PassThruUInt32SizedSection<T>(data);
    }

    public PassThruMagicUInt32SizedSection(
        string magic,
        T data,
        int tweakSize) {
      this.Magic = magic;
      this.impl_ = new PassThruUInt32SizedSection<T>(data, tweakSize);
    }

    [Ignore]
    public uint Size => this.impl_.Size;

    [Ignore]
    public T Data => this.impl_.Data;
  }
}