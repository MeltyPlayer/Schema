using System.Linq;

using schema.binary.util;

namespace System.IO {
  public sealed partial class FinTextReader {
    public void AssertHexByte(byte expectedValue)
      => Asserts.Equal(expectedValue, this.ReadHexByte());

    public byte ReadHexByte() => this.ConvertHexByte_(this.ReadHexChars_());


    public void AssertHexSByte(sbyte expectedValue)
      => Asserts.Equal(expectedValue, this.ReadHexSByte());

    public sbyte ReadHexSByte() => ConvertHexSByte_(this.ReadHexChars_());


    public void AssertHexInt16(short expectedValue)
      => Asserts.Equal(expectedValue, this.ReadHexInt16());

    public short ReadHexInt16() => ConvertHexInt16_(this.ReadHexChars_());

    public void AssertHexUInt16(ushort expectedValue)
      => Asserts.Equal(expectedValue, this.ReadHexUInt16());

    public ushort ReadHexUInt16() => ConvertHexUInt16_(this.ReadHexChars_());


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

    private static readonly string[] hexMatches =
        digitMatches_
            .Concat(
                new[] { "a", "b", "c", "d", "e", "f" }.SelectMany(
                    c => new[] { c.ToLower(), c.ToUpper() }))
            .ToArray();

    private string ReadHexChars_() {
      IgnoreManyIfPresent(TextReaderConstants.WHITESPACE_STRINGS);
      IgnoreOnceIfPresent(hexSpecifierMatches_);
      return this.ReadWhile(FinTextReader.hexMatches);
    }
  }
}