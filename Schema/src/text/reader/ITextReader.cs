using System;


namespace schema.text.reader;

public partial interface ITextReader : ITextStream, IDataReader {
  void AdvanceIfTrue(Func<ITextReader, bool> handler);

  void AssertHexByte(byte expectedValue);
  byte ReadHexByte();

  void AssertHexSByte(sbyte expectedValue);
  sbyte ReadHexSByte();

  void AssertHexInt16(short expectedValue);
  short ReadHexInt16();

  void AssertHexUInt16(ushort expectedValue);
  ushort ReadHexUInt16();

  void AssertHexInt32(int expectedValue);
  int ReadHexInt32();

  void AssertHexUInt32(uint expectedValue);
  uint ReadHexUInt32();

  void AssertHexInt64(long expectedValue);
  long ReadHexInt64();

  void AssertHexUInt64(ulong expectedValue);
  ulong ReadHexUInt64();

  T ReadNew<T>() where T : ITextDeserializable, new();

  bool TryReadNew<T>(out T? value) where T : ITextDeserializable, new();

  void ReadNews<T>(out T[] array, int length)
      where T : ITextDeserializable, new();

  T[] ReadNews<T>(int length) where T : ITextDeserializable, new();
}