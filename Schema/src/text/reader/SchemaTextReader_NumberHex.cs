using System.Linq;

using schema.util.asserts;


namespace schema.text.reader;

public sealed partial class SchemaTextReader {
  public void AssertHexByte(byte expectedValue)
    => Asserts.Equal(expectedValue, this.ReadHexByte());

  public byte ReadHexByte() => this.ConvertHexByte_(this.ReadHexChars_());


  public void AssertHexSByte(sbyte expectedValue)
    => Asserts.Equal(expectedValue, this.ReadHexSByte());

  public sbyte ReadHexSByte() => this.ConvertHexSByte_(this.ReadHexChars_());


  public void AssertHexInt16(short expectedValue)
    => Asserts.Equal(expectedValue, this.ReadHexInt16());

  public short ReadHexInt16() => this.ConvertHexInt16_(this.ReadHexChars_());

  public void AssertHexUInt16(ushort expectedValue)
    => Asserts.Equal(expectedValue, this.ReadHexUInt16());

  public ushort ReadHexUInt16()
    => this.ConvertHexUInt16_(this.ReadHexChars_());


  public void AssertHexInt32(int expectedValue)
    => Asserts.Equal(expectedValue, this.ReadHexInt32());

  public int ReadHexInt32() => this.ConvertHexInt32_(this.ReadHexChars_());


  public void AssertHexUInt32(uint expectedValue)
    => Asserts.Equal(expectedValue, this.ReadHexUInt32());

  public uint ReadHexUInt32() => this.ConvertHexUInt32_(this.ReadHexChars_());


  public void AssertHexInt64(long expectedValue)
    => Asserts.Equal(expectedValue, this.ReadHexInt64());

  public long ReadHexInt64() => this.ConvertHexInt64_(this.ReadHexChars_());


  public void AssertHexUInt64(ulong expectedValue)
    => Asserts.Equal(expectedValue, this.ReadHexUInt64());

  public ulong ReadHexUInt64()
    => this.ConvertHexUInt64_(this.ReadHexChars_());


  private static readonly string[] hexSpecifierMatches_ = { "0x", "0X" };

  private static readonly char[] hexMatches =
      digitMatches_
          .Concat(
              new[] { 'a', 'b', 'c', 'd', 'e', 'f' }.SelectMany(
                  c => new[] { char.ToLower(c), char.ToUpper(c) }))
          .ToArray();

  private string ReadHexChars_() {
    this.SkipManyIfPresent(TextReaderConstants.WHITESPACE_STRINGS);
    this.SkipOnceIfPresent(hexSpecifierMatches_);
    return this.ReadWhile(SchemaTextReader.hexMatches);
  }
}