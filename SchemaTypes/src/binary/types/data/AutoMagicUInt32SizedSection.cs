using schema.binary.attributes;

namespace schema.binary.types.data {
  /// <summary>
  ///   Schema class that implements a uint32-sized section without needing to
  ///   worry about passing in an instance of the contained data. This should
  ///   be adequate for most cases, except when the data class needs to access
  ///   parent data.
  /// </summary>
  [BinarySchema]
  public partial class AutoMagicUInt32SizedSection<T> : IMagicSection<T>
      where T : IBinaryConvertible, new() {
    private readonly PassThruMagicUInt32SizedSection<T> impl_;

    public AutoMagicUInt32SizedSection(string magic) {
      this.impl_ = new(magic, new T());
    }

    public AutoMagicUInt32SizedSection(string magic, int tweakSize) {
      this.impl_ = new(magic, new T(), tweakSize);
    }


    [Ignore]
    public string Magic => this.impl_.Magic;

    [Ignore]
    public uint Size => this.impl_.Size;

    [Ignore]
    public T Data => this.impl_.Data;
  }
}