using System;
using System.IO;

using schema.binary.attributes;

namespace schema.binary.types.data {
  /// <summary>
  ///   Schema class that implements a uint32-sized section without needing to
  ///   worry about passing in an instance of the contained data. This should
  ///   be adequate for most cases, except when the data class needs to access
  ///   parent data.
  /// </summary>
  [BinarySchema]
  public partial class SwitchMagicStringUInt32SizedSection<T> : IMagicSection<T>
      where T : IBinaryConvertible {
    [Ignore]
    private readonly int magicLength_;

    [Ignore]
    private readonly int tweakSize_;

    [Ignore]
    private readonly Func<string, T> createTypeHandler_;

    private PassThruMagicUInt32SizedSection<T> impl_;

    public SwitchMagicStringUInt32SizedSection(
        int magicLength,
        Func<string, T> createTypeHandler) {
      this.magicLength_ = magicLength;
      this.createTypeHandler_ = createTypeHandler;
    }

    public SwitchMagicStringUInt32SizedSection(
        int magicLength,
        int tweakSize,
        Func<string, T> createTypeHandler) {
      this.magicLength_ = magicLength;
      this.tweakSize_ = tweakSize;
      this.createTypeHandler_ = createTypeHandler;
    }

    [Ignore]
    public string Magic => this.impl_.Magic;

    [Ignore]
    public T Data => this.impl_.Data;

    public void Read(IEndianBinaryReader er) {
      var baseOffset = er.Position;

      var magic = er.ReadString(this.magicLength_);
      this.impl_ =
          new PassThruMagicUInt32SizedSection<T>(
              magic,
              this.createTypeHandler_(magic),
              this.tweakSize_);

      er.Position = baseOffset;
      this.impl_.Read(er);
    }
  }
}