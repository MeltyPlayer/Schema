using System;


namespace schema.text.reader;

public partial interface ITextReader : ITextStream, IDataReader {
  bool Matches(out string text, ReadOnlySpan<string> matches);

  string ReadUpToStartOfTerminator(ReadOnlySpan<string> terminators);
  string ReadUpToAndPastTerminator(ReadOnlySpan<string> terminators);

  string ReadWhile(ReadOnlySpan<string> matches);

  void SkipOnceIfPresent(ReadOnlySpan<string> matches);
  void SkipManyIfPresent(ReadOnlySpan<string> matches);

  byte[] ReadBytes(ReadOnlySpan<string> separators,
                   ReadOnlySpan<string> terminators);

  byte?[] ReadBytesIncludingEmpty(ReadOnlySpan<string> separators,
                                  ReadOnlySpan<string> terminators);

  byte[] ReadHexBytes(ReadOnlySpan<string> separators,
                      ReadOnlySpan<string> terminators);

  byte?[] ReadHexBytesIncludingEmpty(ReadOnlySpan<string> separators,
                                     ReadOnlySpan<string> terminators);

  sbyte[] ReadSBytes(ReadOnlySpan<string> separators,
                     ReadOnlySpan<string> terminators);

  sbyte?[]
      ReadSBytesIncludingEmpty(ReadOnlySpan<string> separators,
                               ReadOnlySpan<string> terminators);
  sbyte[] ReadHexSBytes(ReadOnlySpan<string> separators,
                        ReadOnlySpan<string> terminators);

  sbyte?[] ReadHexSBytesIncludingEmpty(ReadOnlySpan<string> separators,
                                       ReadOnlySpan<string> terminators);

  short[] ReadInt16s(ReadOnlySpan<string> separators,
                     ReadOnlySpan<string> terminators);

  short?[] ReadInt16sIncludingEmpty(ReadOnlySpan<string> separators,
                                    ReadOnlySpan<string> terminators);

  short[] ReadHexInt16s(ReadOnlySpan<string> separators,
                        ReadOnlySpan<string> terminators);

  short?[] ReadHexInt16sIncludingEmpty(ReadOnlySpan<string> separators,
                                       ReadOnlySpan<string> terminators);

  ushort[] ReadUInt16s(ReadOnlySpan<string> separators,
                       ReadOnlySpan<string> terminators);

  ushort?[] ReadUInt16sIncludingEmpty(ReadOnlySpan<string> separators,
                                      ReadOnlySpan<string> terminators);

  ushort[] ReadHexUInt16s(ReadOnlySpan<string> separators,
                          ReadOnlySpan<string> terminators);

  ushort?[] ReadHexUInt16sIncludingEmpty(ReadOnlySpan<string> separators,
                                         ReadOnlySpan<string> terminators);

  int[] ReadInt32s(ReadOnlySpan<string> separators,
                   ReadOnlySpan<string> terminators);

  int?[] ReadInt32sIncludingEmpty(ReadOnlySpan<string> separators,
                                  ReadOnlySpan<string> terminators);

  int[] ReadHexInt32s(ReadOnlySpan<string> separators,
                      ReadOnlySpan<string> terminators);

  int?[] ReadHexInt32sIncludingEmpty(ReadOnlySpan<string> separators,
                                     ReadOnlySpan<string> terminators);

  uint[] ReadUInt32s(ReadOnlySpan<string> separators,
                     ReadOnlySpan<string> terminators);

  uint?[] ReadUInt32sIncludingEmpty(ReadOnlySpan<string> separators,
                                    ReadOnlySpan<string> terminators);

  uint[] ReadHexUInt32s(ReadOnlySpan<string> separators,
                        ReadOnlySpan<string> terminators);

  uint?[] ReadHexUInt32sIncludingEmpty(ReadOnlySpan<string> separators,
                                       ReadOnlySpan<string> terminators);

  long[] ReadInt64s(ReadOnlySpan<string> separators,
                    ReadOnlySpan<string> terminators);

  long?[] ReadInt64sIncludingEmpty(ReadOnlySpan<string> separators,
                                   ReadOnlySpan<string> terminators);

  long[] ReadHexInt64s(ReadOnlySpan<string> separators,
                       ReadOnlySpan<string> terminators);

  long?[] ReadHexInt64sIncludingEmpty(ReadOnlySpan<string> separators,
                                      ReadOnlySpan<string> terminators);

  ulong[] ReadUInt64s(ReadOnlySpan<string> separators,
                      ReadOnlySpan<string> terminators);

  ulong?[] ReadUInt64sIncludingEmpty(ReadOnlySpan<string> separators,
                                     ReadOnlySpan<string> terminators);

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
}