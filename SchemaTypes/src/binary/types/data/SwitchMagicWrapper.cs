using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace schema.binary.types.data {
  /// <summary>
  ///   Schema class that implements a uint32-sized section without needing to
  ///   worry about passing in an instance of the contained data. This should
  ///   be adequate for most cases, except when the data class needs to access
  ///   parent data.
  /// </summary>
  public class SwitchMagicWrapper<TMagic, TData> : IBinaryConvertible
      where TData : IBinaryConvertible {
    private readonly Func<IEndianBinaryReader, TMagic> readMagicHandler_;
    private readonly Action<ISubEndianBinaryWriter, TMagic> writeMagicHandler_;
    private readonly Func<TMagic, TData> createTypeHandler_;

    public SwitchMagicWrapper(
        Func<IEndianBinaryReader, TMagic> readMagicHandler,
        Action<ISubEndianBinaryWriter, TMagic> writeMagicHandler,
        Func<TMagic, TData> createTypeHandler) {
      this.readMagicHandler_ = readMagicHandler;
      this.writeMagicHandler_ = writeMagicHandler;
      this.createTypeHandler_ = createTypeHandler;
    }

    public TMagic Magic { get; private set; }

    public TData Data { get; private set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Read(IEndianBinaryReader er) {
      this.Magic = this.readMagicHandler_(er);
      this.Data = this.createTypeHandler_(this.Magic);
      this.Data.Read(er);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ISubEndianBinaryWriter ew) {
      this.writeMagicHandler_(ew, this.Magic);
      this.Data.Write(ew);
    }

    public override string ToString() => $"[{this.Magic}]: {this.Data}";
  }
}