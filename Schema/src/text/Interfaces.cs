using schema.text.reader;
using schema.text.writer;

namespace schema.text {
  public interface ITextDeserializable {
    void Read(ITextReader tr);
  }

  public interface ITextSerializable {
    void Write(ITextWriter tw);
  }

  public interface ITextConvertible : ITextDeserializable, ITextSerializable { }
}