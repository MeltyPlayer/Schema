using System;

using schema.util.streams;


namespace schema;

public interface IDataReader : ISeekableStream {
  bool Eof { get; }

  void AssertByte(byte expectedValue);
  byte ReadByte();

  void AssertSByte(sbyte expectedValue);
  sbyte ReadSByte();

  void AssertInt16(short expectedValue);
  short ReadInt16();

  void AssertUInt16(ushort expectedValue);
  ushort ReadUInt16();

  void AssertInt32(int expectedValue);
  int ReadInt32();

  void AssertUInt32(uint expectedValue);
  uint ReadUInt32();

  void AssertInt64(long expectedValue);
  long ReadInt64();

  void AssertUInt64(ulong expectedValue);
  ulong ReadUInt64();

  void AssertSingle(float expectedValue);
  float ReadSingle();

  void AssertDouble(double expectedValue);
  double ReadDouble();

  void AssertChar(char expectedValue);
  char ReadChar();
  char[] ReadChars(long count);
  void ReadChars(Span<char> dst);

  void AssertString(string expectedValue);
  string ReadString(long count);
  string ReadLine();
}

public interface IDataWriter {
  void WriteByte(byte value);
  void WriteSByte(sbyte value);
  void WriteInt16(short value);
  void WriteUInt16(ushort value);
  void WriteInt32(int value);
  void WriteUInt32(uint value);
  void WriteSingle(float value);
  void WriteDouble(double value);

  void WriteChar(char value);
  void WriteChars(ReadOnlySpan<char> values);

  void WriteString(string value);
}