using System;
using System.Linq;

using schema.util.asserts;
using schema.util.strings;


namespace schema.text.reader;

public sealed partial class SchemaTextReader {
  public void AssertByte(byte expectedValue)
    => Asserts.Equal(expectedValue, this.ReadByte());

  public byte ReadByte() {
    var sb = new ValueStringBuilder();
    this.ReadPositiveIntegerChars_(ref sb);
    var value = this.ConvertByte_(sb.AsSpan());
    sb.Dispose();
    return value;
  }


  public void AssertSByte(sbyte expectedValue)
    => Asserts.Equal(expectedValue, this.ReadSByte());

  public sbyte ReadSByte() {
    var sb = new ValueStringBuilder();
    this.ReadNegativeIntegerChars_(ref sb);
    var value = this.ConvertSByte_(sb.AsSpan());
    sb.Dispose();
    return value;
  }


  public void AssertInt16(short expectedValue)
    => Asserts.Equal(expectedValue, this.ReadInt16());

  public short ReadInt16() {
    var sb = new ValueStringBuilder();
    this.ReadNegativeIntegerChars_(ref sb);
    var value = this.ConvertInt16_(sb.AsSpan());
    sb.Dispose();
    return value;
  }

  public void AssertUInt16(ushort expectedValue)
    => Asserts.Equal(expectedValue, this.ReadUInt16());


  public ushort ReadUInt16() {
    var sb = new ValueStringBuilder();
    this.ReadPositiveIntegerChars_(ref sb);
    var value = this.ConvertUInt16_(sb.AsSpan());
    sb.Dispose();
    return value;
  }


  public void AssertInt32(int expectedValue)
    => Asserts.Equal(expectedValue, this.ReadInt32());

  public int ReadInt32() {
    var sb = new ValueStringBuilder();
    this.ReadNegativeIntegerChars_(ref sb);
    var value = this.ConvertInt32_(sb.AsSpan());
    sb.Dispose();
    return value;
  }


  public void AssertUInt32(uint expectedValue)
    => Asserts.Equal(expectedValue, this.ReadUInt32());

  public uint ReadUInt32() {
    var sb = new ValueStringBuilder();
    this.ReadPositiveIntegerChars_(ref sb);
    var value = this.ConvertUInt32_(sb.AsSpan());
    sb.Dispose();
    return value;
  }


  public void AssertInt64(long expectedValue)
    => Asserts.Equal(expectedValue, this.ReadInt64());

  public long ReadInt64() {
    var sb = new ValueStringBuilder();
    this.ReadNegativeIntegerChars_(ref sb);
    var value = this.ConvertInt64_(sb.AsSpan());
    sb.Dispose();
    return value;
  }


  public void AssertUInt64(ulong expectedValue)
    => Asserts.Equal(expectedValue, this.ReadUInt64());

  public ulong ReadUInt64() {
    var sb = new ValueStringBuilder();
    this.ReadPositiveIntegerChars_(ref sb);
    var value = this.ConvertUInt64_(sb.AsSpan());
    sb.Dispose();
    return value;
  }


  public void AssertSingle(float expectedValue)
    => Asserts.Equal(expectedValue, this.ReadSingle());

  public float ReadSingle() {
    var sb = new ValueStringBuilder();
    this.ReadFloatChars_(ref sb);
    var value = this.ConvertSingle_(sb.AsSpan());
    sb.Dispose();
    return value;
  }


  public void AssertDouble(double expectedValue)
    => Asserts.Equal(expectedValue, this.ReadDouble());

  public double ReadDouble() {
    var sb = new ValueStringBuilder();
    this.ReadFloatChars_(ref sb);
    var value = this.ConvertDouble_(sb.AsSpan());
    sb.Dispose();
    return value;
  }

  private static readonly char[] DIGIT_MATCHES_ = [
      '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
  ];

  private static readonly char[] POSITIVE_INTEGER_MATCHES_ =
      DIGIT_MATCHES_;

  private static readonly char[] NEGATIVE_INTEGER_MATCHES_ =
      POSITIVE_INTEGER_MATCHES_.Concat(['-']).ToArray();


  private static readonly char[] FLOAT_MATCHES_ =
      NEGATIVE_INTEGER_MATCHES_.Concat(['.']).ToArray();

  private void ReadPositiveIntegerChars_(ref ValueStringBuilder sb)
    => this.ReadMatchingNonWhitespaceChars_(
        ref sb,
        POSITIVE_INTEGER_MATCHES_);

  private void ReadNegativeIntegerChars_(ref ValueStringBuilder sb)
    => this.ReadMatchingNonWhitespaceChars_(
        ref sb,
        NEGATIVE_INTEGER_MATCHES_);

  private void ReadFloatChars_(ref ValueStringBuilder sb)
    => this.ReadMatchingNonWhitespaceChars_(
        ref sb,
        FLOAT_MATCHES_);

  private void ReadMatchingNonWhitespaceChars_(
      ref ValueStringBuilder sb,
      ReadOnlySpan<char> matches) {
    this.SkipManyIfPresent(TextReaderConstants.TERMINATORS);
    this.ReadWhileImpl_(ref sb, matches);
    this.SkipManyIfPresent(TextReaderConstants.TERMINATORS);
  }
}