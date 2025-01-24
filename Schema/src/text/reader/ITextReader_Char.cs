namespace schema.text.reader;

public partial interface ITextReader : ITextStream, IDataReader {
  bool Matches(char match);

  string ReadUpToStartOfTerminator(char terminator);
  string ReadUpToAndPastTerminator(char terminator);

  string ReadWhile(char match);

  void SkipOnceIfPresent(char match);
  void SkipManyIfPresent(char match);

  byte[] ReadBytes(char separator, char terminator);

  byte?[] ReadBytesIncludingEmpty(char separator,
                                  char terminator);

  byte[] ReadHexBytes(char separator, char terminator);
  byte?[] ReadHexBytesIncludingEmpty(char separator, char terminator);

  sbyte[] ReadSBytes(char separator,
                     char terminator);

  sbyte?[] ReadSBytesIncludingEmpty(char separator, char terminator);

  sbyte[] ReadHexSBytes(char separator, char terminator);
  sbyte?[] ReadHexSBytesIncludingEmpty(char separator, char terminator);

  short[] ReadInt16s(char separator, char terminator);
  short?[] ReadInt16sIncludingEmpty(char separator, char terminator);

  short[] ReadHexInt16s(char separator, char terminator);
  short?[] ReadHexInt16sIncludingEmpty(char separator, char terminator);

  ushort[] ReadUInt16s(char separator, char terminator);
  ushort?[] ReadUInt16sIncludingEmpty(char separator, char terminator);

  ushort[] ReadHexUInt16s(char separator, char terminator);
  ushort?[] ReadHexUInt16sIncludingEmpty(char separator, char terminator);

  int[] ReadInt32s(char separator, char terminator);
  int?[] ReadInt32sIncludingEmpty(char separator, char terminator);

  int[] ReadHexInt32s(char separator, char terminator);
  int?[] ReadHexInt32sIncludingEmpty(char separator, char terminator);

  uint[] ReadUInt32s(char separator, char terminator);
  uint?[] ReadUInt32sIncludingEmpty(char separator, char terminator);

  uint[] ReadHexUInt32s(char separator, char terminator);
  uint?[] ReadHexUInt32sIncludingEmpty(char separator, char terminator);

  long[] ReadInt64s(char separator, char terminator);
  long?[] ReadInt64sIncludingEmpty(char separator, char terminator);

  long[] ReadHexInt64s(char separator,
                       char terminator);

  long?[] ReadHexInt64sIncludingEmpty(char separator,
                                      char terminator);

  ulong[] ReadUInt64s(char separator,
                      char terminator);

  ulong?[] ReadUInt64sIncludingEmpty(char separator,
                                     char terminator);

  ulong[] ReadHexUInt64s(char separator,
                         char terminator);

  ulong?[] ReadHexUInt64sIncludingEmpty(char separator,
                                        char terminator);

  float[] ReadSingles(char separator,
                      char terminator);

  float?[] ReadSinglesIncludingEmpty(char separator,
                                     char terminator);

  double[] ReadDoubles(char separator,
                       char terminator);

  double?[] ReadDoublesIncludingEmpty(char separator,
                                      char terminator);

  string[] ReadStrings(char separator,
                       char terminator);
}