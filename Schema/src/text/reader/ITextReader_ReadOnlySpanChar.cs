using System;


namespace schema.text.reader;

public partial interface ITextReader : ITextStream, IDataReader {
  bool Matches(out char c, ReadOnlySpan<char> matches);

  string ReadUpToStartOfTerminator(ReadOnlySpan<char> terminators);
  string ReadUpToAndPastTerminator(ReadOnlySpan<char> terminators);

  string ReadWhile(ReadOnlySpan<char> matches);

  void SkipOnceIfPresent(ReadOnlySpan<char> matches);
  void SkipManyIfPresent(ReadOnlySpan<char> matches);

  byte[] ReadBytes(ReadOnlySpan<char> separators,
                   ReadOnlySpan<char> terminators);

  byte?[] ReadBytesIncludingEmpty(ReadOnlySpan<char> separators,
                                  ReadOnlySpan<char> terminators);

  byte[] ReadHexBytes(ReadOnlySpan<char> separators,
                      ReadOnlySpan<char> terminators);

  byte?[] ReadHexBytesIncludingEmpty(ReadOnlySpan<char> separators,
                                     ReadOnlySpan<char> terminators);

  sbyte[] ReadSBytes(ReadOnlySpan<char> separators,
                     ReadOnlySpan<char> terminators);

  sbyte?[]
      ReadSBytesIncludingEmpty(ReadOnlySpan<char> separators,
                               ReadOnlySpan<char> terminators);
  sbyte[] ReadHexSBytes(ReadOnlySpan<char> separators,
                        ReadOnlySpan<char> terminators);

  sbyte?[] ReadHexSBytesIncludingEmpty(ReadOnlySpan<char> separators,
                                       ReadOnlySpan<char> terminators);

  short[] ReadInt16s(ReadOnlySpan<char> separators,
                     ReadOnlySpan<char> terminators);

  short?[] ReadInt16sIncludingEmpty(ReadOnlySpan<char> separators,
                                    ReadOnlySpan<char> terminators);

  short[] ReadHexInt16s(ReadOnlySpan<char> separators,
                        ReadOnlySpan<char> terminators);

  short?[] ReadHexInt16sIncludingEmpty(ReadOnlySpan<char> separators,
                                       ReadOnlySpan<char> terminators);

  ushort[] ReadUInt16s(ReadOnlySpan<char> separators,
                       ReadOnlySpan<char> terminators);

  ushort?[] ReadUInt16sIncludingEmpty(ReadOnlySpan<char> separators,
                                      ReadOnlySpan<char> terminators);

  ushort[] ReadHexUInt16s(ReadOnlySpan<char> separators,
                          ReadOnlySpan<char> terminators);

  ushort?[] ReadHexUInt16sIncludingEmpty(ReadOnlySpan<char> separators,
                                         ReadOnlySpan<char> terminators);

  int[] ReadInt32s(ReadOnlySpan<char> separators,
                   ReadOnlySpan<char> terminators);

  int?[] ReadInt32sIncludingEmpty(ReadOnlySpan<char> separators,
                                  ReadOnlySpan<char> terminators);

  int[] ReadHexInt32s(ReadOnlySpan<char> separators,
                      ReadOnlySpan<char> terminators);

  int?[] ReadHexInt32sIncludingEmpty(ReadOnlySpan<char> separators,
                                     ReadOnlySpan<char> terminators);

  uint[] ReadUInt32s(ReadOnlySpan<char> separators,
                     ReadOnlySpan<char> terminators);

  uint?[] ReadUInt32sIncludingEmpty(ReadOnlySpan<char> separators,
                                    ReadOnlySpan<char> terminators);

  uint[] ReadHexUInt32s(ReadOnlySpan<char> separators,
                        ReadOnlySpan<char> terminators);

  uint?[] ReadHexUInt32sIncludingEmpty(ReadOnlySpan<char> separators,
                                       ReadOnlySpan<char> terminators);

  long[] ReadInt64s(ReadOnlySpan<char> separators,
                    ReadOnlySpan<char> terminators);

  long?[] ReadInt64sIncludingEmpty(ReadOnlySpan<char> separators,
                                   ReadOnlySpan<char> terminators);

  long[] ReadHexInt64s(ReadOnlySpan<char> separators,
                       ReadOnlySpan<char> terminators);

  long?[] ReadHexInt64sIncludingEmpty(ReadOnlySpan<char> separators,
                                      ReadOnlySpan<char> terminators);

  ulong[] ReadUInt64s(ReadOnlySpan<char> separators,
                      ReadOnlySpan<char> terminators);

  ulong?[] ReadUInt64sIncludingEmpty(ReadOnlySpan<char> separators,
                                     ReadOnlySpan<char> terminators);

  ulong[] ReadHexUInt64s(ReadOnlySpan<char> separators,
                         ReadOnlySpan<char> terminators);

  ulong?[] ReadHexUInt64sIncludingEmpty(ReadOnlySpan<char> separators,
                                        ReadOnlySpan<char> terminators);

  float[] ReadSingles(ReadOnlySpan<char> separators,
                      ReadOnlySpan<char> terminators);

  float?[] ReadSinglesIncludingEmpty(ReadOnlySpan<char> separators,
                                     ReadOnlySpan<char> terminators);

  double[] ReadDoubles(ReadOnlySpan<char> separators,
                       ReadOnlySpan<char> terminators);

  double?[] ReadDoublesIncludingEmpty(ReadOnlySpan<char> separators,
                                      ReadOnlySpan<char> terminators);

  string[] ReadStrings(ReadOnlySpan<char> separators,
                       ReadOnlySpan<char> terminators);
}