using System.IO;


namespace schema.binary {
  public interface IBinary { }

  public interface IBinarySerializable : IBinary {
    void Write(IBinaryWriter ew);
  }

  public interface IBinaryDeserializable : IBinary {
    void Read(IBinaryReader br);
  }

  public interface IBinaryConvertible : IBinarySerializable,
                                        IBinaryDeserializable { }
}