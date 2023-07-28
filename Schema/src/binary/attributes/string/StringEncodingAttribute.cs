using System;
using System.IO;
using System.Text;


namespace schema.binary.attributes {
  public enum StringEncodingType {
    ASCII,
    UTF8,
    UTF16,
    UTF32,
  }

  public static class StringEncodingTypeExtensions {
    public static Encoding GetEncoding(
        this StringEncodingType stringEncodingType,
        Endianness endianness)
      => stringEncodingType switch {
          StringEncodingType.ASCII => Encoding.ASCII,
          StringEncodingType.UTF8  => Encoding.UTF8,
          StringEncodingType.UTF16 => endianness switch {
              Endianness.BigEndian    => Encoding.BigEndianUnicode,
              Endianness.LittleEndian => Encoding.Unicode,
          },
          StringEncodingType.UTF32 => Encoding.UTF32,
      };
  }

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class StringEncodingAttribute : BMemberAttribute<string> {
    public StringEncodingAttribute(StringEncodingType encodingType) {
      this.EncodingType = encodingType;
    }

    protected override void InitFields() { }

    public StringEncodingType EncodingType { get; private set; }
  }
}