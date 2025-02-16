﻿using System;
using System.Linq;

using schema.util.asserts;


namespace schema.text.reader;

public sealed partial class SchemaTextReader {
  public void AssertByte(byte expectedValue)
    => Asserts.Equal(expectedValue, this.ReadByte());

  public byte ReadByte()
    => this.ConvertByte_(this.ReadPositiveIntegerChars_());


  public void AssertSByte(sbyte expectedValue)
    => Asserts.Equal(expectedValue, this.ReadSByte());

  public sbyte ReadSByte()
    => this.ConvertSByte_(this.ReadNegativeIntegerChars_());


  public void AssertInt16(short expectedValue)
    => Asserts.Equal(expectedValue, this.ReadInt16());

  public short ReadInt16()
    => this.ConvertInt16_(this.ReadPositiveIntegerChars_());

  public void AssertUInt16(ushort expectedValue)
    => Asserts.Equal(expectedValue, this.ReadUInt16());


  public ushort ReadUInt16()
    => this.ConvertUInt16_(this.ReadNegativeIntegerChars_());


  public void AssertInt32(int expectedValue)
    => Asserts.Equal(expectedValue, this.ReadInt32());

  public int ReadInt32()
    => this.ConvertInt32_(this.ReadPositiveIntegerChars_());


  public void AssertUInt32(uint expectedValue)
    => Asserts.Equal(expectedValue, this.ReadUInt32());

  public uint ReadUInt32()
    => this.ConvertUInt32_(this.ReadNegativeIntegerChars_());


  public void AssertInt64(long expectedValue)
    => Asserts.Equal(expectedValue, this.ReadInt64());

  public long ReadInt64()
    => this.ConvertInt64_(this.ReadPositiveIntegerChars_());


  public void AssertUInt64(ulong expectedValue)
    => Asserts.Equal(expectedValue, this.ReadUInt64());

  public ulong ReadUInt64()
    => this.ConvertUInt64_(this.ReadNegativeIntegerChars_());


  public void AssertSingle(float expectedValue)
    => Asserts.Equal(expectedValue, this.ReadSingle());

  public float ReadSingle() => this.ConvertSingle_(this.ReadFloatChars_());


  public void AssertDouble(double expectedValue)
    => Asserts.Equal(expectedValue, this.ReadDouble());

  public double ReadDouble() => this.ConvertDouble_(this.ReadFloatChars_());

  private static readonly char[] digitMatches_ = [
      '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
  ];

  private static readonly char[] positiveIntegerMatches_ =
      digitMatches_;

  private static readonly char[] negativeIntegerMatches_ =
      positiveIntegerMatches_.Concat(['-']).ToArray();


  private static readonly char[] floatMatches_ =
      negativeIntegerMatches_.Concat(['.']).ToArray();

  private string ReadPositiveIntegerChars_()
    => this.ReadMatchingNonWhitespaceChars_(
        SchemaTextReader.positiveIntegerMatches_);

  private string ReadNegativeIntegerChars_()
    => this.ReadMatchingNonWhitespaceChars_(
        SchemaTextReader.negativeIntegerMatches_);

  private string ReadFloatChars_()
    => this.ReadMatchingNonWhitespaceChars_(SchemaTextReader.floatMatches_);

  private string ReadMatchingNonWhitespaceChars_(ReadOnlySpan<char> matches) {
    this.SkipManyIfPresent(TextReaderConstants.TERMINATORS);
    var matching = this.ReadWhile(matches);
    this.SkipManyIfPresent(TextReaderConstants.TERMINATORS);
    return matching;
  }
}