using System.IO;

namespace schema.binary {
  public interface IBinarySerializable {
    void Write(ISubEndianBinaryWriter ew);
  }

  public interface IBinaryDeserializable {
    void Read(IEndianBinaryReader er);
  }

  public interface IBinaryConvertible : IBinarySerializable,
                                        IBinaryDeserializable { }
}