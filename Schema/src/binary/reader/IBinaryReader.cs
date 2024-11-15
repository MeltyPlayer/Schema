using System;
using System.Text;

using schema.binary.attributes;
using schema.util.streams;


namespace schema.binary;

public interface IBinaryReader
    : IDataReader,
      IEndiannessStack,
      ILocalSpaceStack,
      IReadableStream {
  void Close();

  void AssertPosition(long expectedPosition);
  void AssertNotEof();

  void Align(uint amt);

  void Subread(int len, Action<IBinaryReader> subread);
  T Subread<T>(int len, Func<IBinaryReader, T> subread);

  void SubreadAt(long position, Action<IBinaryReader> subread);

  void SubreadAt(long position,
                 int len,
                 Action<IBinaryReader> subread);

  T SubreadAt<T>(long position, Func<IBinaryReader, T> subread);

  T SubreadAt<T>(long position,
                 int len,
                 Func<IBinaryReader, T> subread);

  void FillBuffer(Span<byte> data, int stride);

  new byte ReadByte();

  byte[] ReadBytes(long count);
  void ReadBytes(Span<byte> dst);

  sbyte[] ReadSBytes(long count);
  void ReadSBytes(Span<sbyte> dst);

  short[] ReadInt16s(long count);
  void ReadInt16s(Span<short> dst);

  ushort[] ReadUInt16s(long count);
  void ReadUInt16s(Span<ushort> dst);

  void AssertInt24(int expectedValue);
  int ReadInt24();
  int[] ReadInt24s(long count);
  void ReadInt24s(Span<int> dst);

  void AssertUInt24(uint expectedValue);
  uint ReadUInt24();
  uint[] ReadUInt24s(long count);
  void ReadUInt24s(Span<uint> dst);

  int[] ReadInt32s(long count);
  void ReadInt32s(Span<int> dst);

  uint[] ReadUInt32s(long count);
  void ReadUInt32s(Span<uint> dst);

  long[] ReadInt64s(long count);
  void ReadInt64s(Span<long> dst);

  ulong[] ReadUInt64s(long count);
  void ReadUInt64s(Span<ulong> dst);

  void AssertHalf(float expectedValue);
  float ReadHalf();
  float[] ReadHalfs(long count);
  void ReadHalfs(Span<float> dst);

  float[] ReadSingles(long count);
  void ReadSingles(Span<float> dst);

  double[] ReadDoubles(long count);
  void ReadDoubles(Span<double> dst);

  void AssertSn8(float expectedValue);
  float ReadSn8();
  float[] ReadSn8s(long count);
  void ReadSn8s(Span<float> dst);

  void AssertUn8(float expectedValue);
  float ReadUn8();
  float[] ReadUn8s(long count);
  void ReadUn8s(Span<float> dst);

  void AssertSn16(float expectedValue);
  float ReadSn16();
  float[] ReadSn16s(long count);
  void ReadSn16s(Span<float> dst);

  void AssertUn16(float expectedValue);
  float ReadUn16();
  float[] ReadUn16s(long count);
  void ReadUn16s(Span<float> dst);

  void AssertChar(StringEncodingType encodingType, char expectedValue);
  char ReadChar(StringEncodingType encodingType);
  char[] ReadChars(StringEncodingType encodingType, long count);
  void ReadChars(StringEncodingType encodingType, Span<char> dst);

  void AssertChar(Encoding encoding, char expectedValue);
  char ReadChar(Encoding encoding);
  char[] ReadChars(Encoding encoding, long count);
  void ReadChars(Encoding encoding, Span<char> dst);

  string ReadUpTo(char endToken);
  string ReadUpTo(StringEncodingType encodingType, char endToken);
  string ReadUpTo(Encoding encoding, char endToken);

  string ReadUpTo(ReadOnlySpan<string> endTokens);

  string ReadUpTo(StringEncodingType encodingType,
                  ReadOnlySpan<string> endTokens);

  string ReadUpTo(Encoding encoding, ReadOnlySpan<string> endTokens);

  string ReadLine(StringEncodingType encodingType);
  string ReadLine(Encoding encoding);

  void AssertString(StringEncodingType encodingType, string expectedValue);
  string ReadString(StringEncodingType encodingType, long count);

  void AssertString(Encoding encoding, string expectedValue);
  string ReadString(Encoding encoding, long count);

  void AssertStringNT(string expectedValue);
  string ReadStringNT();

  void AssertStringNT(StringEncodingType encodingType, string expectedValue);
  string ReadStringNT(StringEncodingType encodingType);

  void AssertStringNT(Encoding encoding, string expectedValue);
  string ReadStringNT(Encoding encoding);

  T ReadNew<T>() where T : IBinaryDeserializable, new();
  bool TryReadNew<T>(out T? value) where T : IBinaryDeserializable, new();
  T[] ReadNews<T>(int length) where T : IBinaryDeserializable, new();
  void ReadNews<T>(Span<T> dst) where T : IBinaryDeserializable, new();
}