using schema.binary;

using System.Text;

namespace System.IO {
  public interface IEndianBinaryReader : IEndiannessStack, IDataReader {
    void Close();

    void AssertPosition(long expectedPosition);
    void AssertNotEof();

    void Align(uint amt);

    byte[] ReadBytesAtOffset(long position, int len);
    string ReadStringAtOffset(long position, int len);
    string ReadStringNTAtOffset(long position);
    T ReadNewAtOffset<T>(long position) where T : IBinaryDeserializable, new();
    T[] ReadNewArrayAtOffset<T>(long position, int length) where T : IBinaryDeserializable, new();

    void Subread(long position,
                 int len,
                 Action<IEndianBinaryReader> subread);

    void Subread(long position, Action<IEndianBinaryReader> subread);

    byte[] ReadBytes(long count);
    void ReadBytes(byte[] dst, int start, int length);
    void ReadBytes(Span<byte> dst);

    sbyte[] ReadSBytes(long count);
    void ReadSBytes(sbyte[] dst, int start, int length);
    void ReadSBytes(Span<sbyte> dst);

    short[] ReadInt16s(long count);
    void ReadInt16s(short[] dst, int start, int length);
    void ReadInt16s(Span<short> dst);

    ushort[] ReadUInt16s(long count);
    void ReadUInt16s(ushort[] dst, int start, int length);
    void ReadUInt16s(Span<ushort> dst);

    void AssertInt24(int expectedValue);
    int ReadInt24();
    int[] ReadInt24s(long count);
    void ReadInt24s(int[] dst, int start, int length);
    void ReadInt24s(Span<int> dst);

    void AssertUInt24(uint expectedValue);
    uint ReadUInt24();
    uint[] ReadUInt24s(long count);
    void ReadUInt24s(uint[] dst, int start, int length);
    void ReadUInt24s(Span<uint> dst);

    int[] ReadInt32s(long count);
    void ReadInt32s(int[] dst, int start, int length);
    void ReadInt32s(Span<int> dst);

    uint[] ReadUInt32s(long count);
    void ReadUInt32s(uint[] dst, int start, int length);
    void ReadUInt32s(Span<uint> dst);

    long[] ReadInt64s(long count);
    void ReadInt64s(long[] dst, int start, int length);
    void ReadInt64s(Span<long> dst);

    ulong[] ReadUInt64s(long count);
    void ReadUInt64s(ulong[] dst, int start, int length);
    void ReadUInt64s(Span<ulong> dst);

    void AssertHalf(float expectedValue);
    float ReadHalf();
    float[] ReadHalfs(long count);
    void ReadHalfs(float[] dst, int start, int length);
    void ReadHalfs(Span<float> dst);

    float[] ReadSingles(long count);
    void ReadSingles(float[] dst, int start, int length);
    void ReadSingles(Span<float> dst);

    double[] ReadDoubles(long count);
    void ReadDoubles(double[] dst, int start, int length);
    void ReadDoubles(Span<double> dst);

    void AssertSn8(float expectedValue);
    float ReadSn8();
    float[] ReadSn8s(long count);
    void ReadSn8s(float[] dst, int start, int length);
    void ReadSn8s(Span<float> dst);

    void AssertUn8(float expectedValue);
    float ReadUn8();
    float[] ReadUn8s(long count);
    void ReadUn8s(float[] dst, int start, int length);
    void ReadUn8s(Span<float> dst);

    void AssertSn16(float expectedValue);
    float ReadSn16();
    float[] ReadSn16s(long count);
    void ReadSn16s(float[] dst, int start, int length);
    void ReadSn16s(Span<float> dst);

    void AssertUn16(float expectedValue);
    float ReadUn16();
    float[] ReadUn16s(long count);
    void ReadUn16s(float[] dst, int start, int length);
    void ReadUn16s(Span<float> dst);

    void AssertChar(Encoding encoding, char expectedValue);
    char ReadChar(Encoding encoding);
    char[] ReadChars(Encoding encoding, long count);
    void ReadChars(Encoding encoding, char[] dst, int start, int length);
    void ReadChars(Encoding encoding, Span<char> dst);

    string ReadUpTo(char endToken);
    string ReadUpTo(Encoding encoding, char endToken);

    string ReadUpTo(params string[] endTokens);
    string ReadUpTo(Encoding encoding, params string[] endTokens);

    string ReadLine();
    string ReadLine(Encoding encoding);

    void AssertString(Encoding encoding, string expectedValue);
    string ReadString(Encoding encoding, long count);

    void AssertStringNT(string expectedValue);
    string ReadStringNT();

    void AssertStringNT(Encoding encoding, string expectedValue);
    string ReadStringNT(Encoding encoding);

    void AssertMagicText(string expectedText);

    T ReadNew<T>() where T : IBinaryDeserializable, new();

    bool TryReadNew<T>(out T? value) where T : IBinaryDeserializable, new();

    void ReadNewArray<T>(out T[] array, int length)
        where T : IBinaryDeserializable, new();

    T[] ReadNewArray<T>(int length) where T : IBinaryDeserializable, new();
  }
}