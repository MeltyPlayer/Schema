using System.IO;

namespace schema.text {
  public interface ITextDeserializable {
    void Read(ITextReader tr);
  }

  public interface ITextConvertible : ITextDeserializable { }
}