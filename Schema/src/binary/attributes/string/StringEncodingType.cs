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
}