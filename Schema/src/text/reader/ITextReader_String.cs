namespace schema.text.reader;

public partial interface ITextReader {
  bool Matches(string match);

  string ReadUpToStartOfTerminator(string terminator);
  string ReadUpToAndPastTerminator(string terminator);

  void SkipUpToStartOfTerminator(string terminator);
  void SkipUpToAndPastTerminator(string terminator);

  string ReadWhile(string match);

  void SkipOnceIfPresent(string match);
  void SkipManyIfPresent(string match);

  byte[] ReadBytes(string separator, string terminator);

  byte?[] ReadBytesIncludingEmpty(string separator,
                                  string terminator);

  byte[] ReadHexBytes(string separator, string terminator);
  byte?[] ReadHexBytesIncludingEmpty(string separator, string terminator);

  sbyte[] ReadSBytes(string separator,
                     string terminator);

  sbyte?[] ReadSBytesIncludingEmpty(string separator, string terminator);

  sbyte[] ReadHexSBytes(string separator, string terminator);
  sbyte?[] ReadHexSBytesIncludingEmpty(string separator, string terminator);

  short[] ReadInt16s(string separator, string terminator);
  short?[] ReadInt16sIncludingEmpty(string separator, string terminator);

  short[] ReadHexInt16s(string separator, string terminator);
  short?[] ReadHexInt16sIncludingEmpty(string separator, string terminator);

  ushort[] ReadUInt16s(string separator, string terminator);
  ushort?[] ReadUInt16sIncludingEmpty(string separator, string terminator);

  ushort[] ReadHexUInt16s(string separator, string terminator);
  ushort?[] ReadHexUInt16sIncludingEmpty(string separator, string terminator);

  int[] ReadInt32s(string separator, string terminator);
  int?[] ReadInt32sIncludingEmpty(string separator, string terminator);

  int[] ReadHexInt32s(string separator, string terminator);
  int?[] ReadHexInt32sIncludingEmpty(string separator, string terminator);

  uint[] ReadUInt32s(string separator, string terminator);
  uint?[] ReadUInt32sIncludingEmpty(string separator, string terminator);

  uint[] ReadHexUInt32s(string separator, string terminator);
  uint?[] ReadHexUInt32sIncludingEmpty(string separator, string terminator);

  long[] ReadInt64s(string separator, string terminator);
  long?[] ReadInt64sIncludingEmpty(string separator, string terminator);

  long[] ReadHexInt64s(string separator,
                       string terminator);

  long?[] ReadHexInt64sIncludingEmpty(string separator,
                                      string terminator);

  ulong[] ReadUInt64s(string separator,
                      string terminator);

  ulong?[] ReadUInt64sIncludingEmpty(string separator,
                                     string terminator);

  ulong[] ReadHexUInt64s(string separator,
                         string terminator);

  ulong?[] ReadHexUInt64sIncludingEmpty(string separator,
                                        string terminator);

  float[] ReadSingles(string separator,
                      string terminator);

  float?[] ReadSinglesIncludingEmpty(string separator,
                                     string terminator);

  double[] ReadDoubles(string separator,
                       string terminator);

  double?[] ReadDoublesIncludingEmpty(string separator,
                                      string terminator);

  string[] ReadStrings(string separator,
                       string terminator);
}