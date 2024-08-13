using System;


namespace schema.text.reader;

public interface ITextReader : ITextStream, IDataReader {
  void AdvanceIfTrue(Func<ITextReader, bool> handler);

  bool Matches(out string text, ReadOnlySpan<string> matches);
  bool Matches(out char match, ReadOnlySpan<char> matches);

  string ReadUpToStartOfTerminator(ReadOnlySpan<string> terminators);
  string ReadUpToAndPastTerminator(ReadOnlySpan<string> terminators);

  string ReadWhile(ReadOnlySpan<char> matches);
  string ReadWhile(ReadOnlySpan<string> matches);

  void SkipOnceIfPresent(ReadOnlySpan<string> matches);
  void SkipManyIfPresent(ReadOnlySpan<string> matches);

  byte[] ReadBytes(ReadOnlySpan<string> separators,
                   ReadOnlySpan<string> terminators);

  byte?[] ReadBytesIncludingEmpty(ReadOnlySpan<string> separators,
                                  ReadOnlySpan<string> terminators);

  void AssertHexByte(byte expectedValue);
  byte ReadHexByte();

  byte[] ReadHexBytes(ReadOnlySpan<string> separators,
                      ReadOnlySpan<string> terminators);

  byte?[] ReadHexBytesIncludingEmpty(ReadOnlySpan<string> separators,
                                     ReadOnlySpan<string> terminators);

  sbyte[] ReadSBytes(ReadOnlySpan<string> separators,
                     ReadOnlySpan<string> terminators);

  sbyte?[]
      ReadSBytesIncludingEmpty(ReadOnlySpan<string> separators,
                               ReadOnlySpan<string> terminators);

  void AssertHexSByte(sbyte expectedValue);
  sbyte ReadHexSByte();

  sbyte[] ReadHexSBytes(ReadOnlySpan<string> separators,
                        ReadOnlySpan<string> terminators);

  sbyte?[] ReadHexSBytesIncludingEmpty(ReadOnlySpan<string> separators,
                                       ReadOnlySpan<string> terminators);

  short[] ReadInt16s(ReadOnlySpan<string> separators,
                     ReadOnlySpan<string> terminators);

  short?[] ReadInt16sIncludingEmpty(ReadOnlySpan<string> separators,
                                    ReadOnlySpan<string> terminators);

  void AssertHexInt16(short expectedValue);
  short ReadHexInt16();

  short[] ReadHexInt16s(ReadOnlySpan<string> separators,
                        ReadOnlySpan<string> terminators);

  short?[] ReadHexInt16sIncludingEmpty(ReadOnlySpan<string> separators,
                                       ReadOnlySpan<string> terminators);

  ushort[] ReadUInt16s(ReadOnlySpan<string> separators,
                       ReadOnlySpan<string> terminators);

  ushort?[] ReadUInt16sIncludingEmpty(ReadOnlySpan<string> separators,
                                      ReadOnlySpan<string> terminators);

  void AssertHexUInt16(ushort expectedValue);
  ushort ReadHexUInt16();

  ushort[] ReadHexUInt16s(ReadOnlySpan<string> separators,
                          ReadOnlySpan<string> terminators);

  ushort?[] ReadHexUInt16sIncludingEmpty(ReadOnlySpan<string> separators,
                                         ReadOnlySpan<string> terminators);

  int[] ReadInt32s(ReadOnlySpan<string> separators,
                   ReadOnlySpan<string> terminators);

  int?[] ReadInt32sIncludingEmpty(ReadOnlySpan<string> separators,
                                  ReadOnlySpan<string> terminators);

  void AssertHexInt32(int expectedValue);
  int ReadHexInt32();

  int[] ReadHexInt32s(ReadOnlySpan<string> separators,
                      ReadOnlySpan<string> terminators);

  int?[] ReadHexInt32sIncludingEmpty(ReadOnlySpan<string> separators,
                                     ReadOnlySpan<string> terminators);

  uint[] ReadUInt32s(ReadOnlySpan<string> separators,
                     ReadOnlySpan<string> terminators);

  uint?[] ReadUInt32sIncludingEmpty(ReadOnlySpan<string> separators,
                                    ReadOnlySpan<string> terminators);

  void AssertHexUInt32(uint expectedValue);
  uint ReadHexUInt32();

  uint[] ReadHexUInt32s(ReadOnlySpan<string> separators,
                        ReadOnlySpan<string> terminators);

  uint?[] ReadHexUInt32sIncludingEmpty(ReadOnlySpan<string> separators,
                                       ReadOnlySpan<string> terminators);

  long[] ReadInt64s(ReadOnlySpan<string> separators,
                    ReadOnlySpan<string> terminators);

  long?[] ReadInt64sIncludingEmpty(ReadOnlySpan<string> separators,
                                   ReadOnlySpan<string> terminators);

  void AssertHexInt64(long expectedValue);
  long ReadHexInt64();

  long[] ReadHexInt64s(ReadOnlySpan<string> separators,
                       ReadOnlySpan<string> terminators);

  long?[] ReadHexInt64sIncludingEmpty(ReadOnlySpan<string> separators,
                                      ReadOnlySpan<string> terminators);

  ulong[] ReadUInt64s(ReadOnlySpan<string> separators,
                      ReadOnlySpan<string> terminators);

  ulong?[] ReadUInt64sIncludingEmpty(ReadOnlySpan<string> separators,
                                     ReadOnlySpan<string> terminators);

  void AssertHexUInt64(ulong expectedValue);
  ulong ReadHexUInt64();

  ulong[] ReadHexUInt64s(ReadOnlySpan<string> separators,
                         ReadOnlySpan<string> terminators);

  ulong?[] ReadHexUInt64sIncludingEmpty(ReadOnlySpan<string> separators,
                                        ReadOnlySpan<string> terminators);

  float[] ReadSingles(ReadOnlySpan<string> separators,
                      ReadOnlySpan<string> terminators);

  float?[] ReadSinglesIncludingEmpty(ReadOnlySpan<string> separators,
                                     ReadOnlySpan<string> terminators);

  double[] ReadDoubles(ReadOnlySpan<string> separators,
                       ReadOnlySpan<string> terminators);

  double?[] ReadDoublesIncludingEmpty(ReadOnlySpan<string> separators,
                                      ReadOnlySpan<string> terminators);

  string[] ReadStrings(ReadOnlySpan<string> separators,
                       ReadOnlySpan<string> terminators);

  T ReadNew<T>() where T : ITextDeserializable, new();

  bool TryReadNew<T>(out T? value) where T : ITextDeserializable, new();

  void ReadNews<T>(out T[] array, int length)
      where T : ITextDeserializable, new();

  T[] ReadNews<T>(int length) where T : ITextDeserializable, new();
}