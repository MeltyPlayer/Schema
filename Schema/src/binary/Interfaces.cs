using System.IO;

namespace schema.binary {
  public interface IBinary { }

  public interface IBinarySerializable : IBinary {
    void Write(ISubBinaryWriter ew);
  }

  public interface IBinaryDeserializable : IBinary {
    void Read(IBinaryReader br);
  }

  public interface IBinaryConvertible : IBinarySerializable,
                                        IBinaryDeserializable { }
}