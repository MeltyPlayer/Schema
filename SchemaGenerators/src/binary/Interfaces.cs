using System.IO;

namespace schema.binary {
  public interface IBinary { }

  public interface IBinarySerializable : IBinary {
    void Write(ISubEndianBinaryWriter ew);
  }

  public interface IBinaryDeserializable : IBinary {
    void Read(IEndianBinaryReader er);
  }

  public interface IBinaryConvertible : IBinarySerializable,
                                        IBinaryDeserializable { }
}