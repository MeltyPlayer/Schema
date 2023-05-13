using System.Text;
using System.Threading.Tasks;


namespace System.IO {
  public interface IEndianBinaryWriter : ISubEndianBinaryWriter, IDisposable {
    Task CompleteAndCopyToDelayed(Stream stream);
  }

  public interface ISubEndianBinaryWriter {
    void Align(uint amt);

    void WriteByte(byte value);
    void WriteBytes(byte[] value);
    void WriteBytes(byte[] value, int offset, int count);

    void WriteSByte(sbyte value);
    void WriteSBytes(sbyte[] value);
    void WriteSBytes(sbyte[] value, int offset, int count);

    void WriteChar(char value);
    void WriteChar(char value, Encoding encoding);
    void WriteChars(char[] value);
    void WriteChars(char[] value, int offset, int count, Encoding encoding);

    void WriteString(string value);
    void WriteStringNT(string value);
    void WriteString(string value, Encoding encoding, bool nullTerminated);

    void WriteStringWithExactLength(string value, int length);
    void WriteStringEndian(string value);
    public void WriteStringEndian(string value, Encoding encoding);

    void WriteDouble(double value);
    void WriteDoubles(double[] value);
    void WriteDoubles(double[] value, int offset, int count);

    void WriteHalf(float value);
    void WriteHalfs(float[] value);
    void WriteHalfs(float[] value, int offset, int count);

    void WriteSingle(float value);
    void WriteSingles(float[] value);
    void WriteSingles(float[] value, int offset, int count);

    void WriteInt24(int value);
    void WriteInt24s(int[] value);
    void WriteInt24s(int[] value, int offset, int count);

    void WriteInt32(int value);
    void WriteInt32s(int[] value);
    void WriteInt32s(int[] value, int offset, int count);

    void WriteInt64(long value);
    void WriteInt64s(long[] value);
    void WriteInt64s(long[] value, int offset, int count);

    void WriteInt16(short value);
    void WriteInt16s(short[] value);
    void WriteInt16s(short[] value, int offset, int count);

    void WriteUInt16(ushort value);
    void WriteUInt16s(ushort[] value);
    void WriteUInt16s(ushort[] value, int offset, int count);

    void WriteUInt24(uint value);
    void WriteUInt24s(uint[] value);
    void WriteUInt24s(uint[] value, int offset, int count);

    void WriteUInt32(uint value);
    void WriteUInt32s(uint[] value);
    void WriteUInt32s(uint[] value, int offset, int count);

    void WriteUInt64(ulong value);
    void WriteUInt64s(ulong[] value);
    void WriteUInt64s(ulong[] value, int offset, int count);

    void WriteUn8(float value);
    void WriteUn8s(float[] value);
    void WriteUn8s(float[] value, int offset, int count);

    void WriteSn8(float value);
    void WriteSn8s(float[] value);
    void WriteSn8s(float[] value, int offset, int count);

    void WriteUn16(float value);
    void WriteUn16s(float[] value);
    void WriteUn16s(float[] value, int offset, int count);

    void WriteSn16(float value);
    void WriteSn16s(float[] value);
    void WriteSn16s(float[] value, int offset, int count);

    void Close();


    // Delayed

    ISubEndianBinaryWriter EnterBlock(out Task<long> delayedLength);

    Task<long> GetAbsolutePosition();
    Task<long> GetAbsoluteLength();

    Task<long> GetStartPositionOfSubStream();
    Task<long> GetPositionInSubStream();
    Task<long> GetLengthOfSubStream();

    void WriteByteDelayed(Task<byte> delayedValue);
    void WriteSByteDelayed(Task<sbyte> delayedValue);
    void WriteInt16Delayed(Task<short> delayedValue);
    void WriteUInt16Delayed(Task<ushort> delayedValue);
    void WriteInt32Delayed(Task<int> delayedValue);
    void WriteUInt32Delayed(Task<uint> delayedValue);
    void WriteInt64Delayed(Task<long> delayedValue);
    void WriteUInt64Delayed(Task<ulong> delayedValue);

    // Endianness

    Endianness Endianness { get; }

    bool IsOppositeEndiannessOfSystem { get; }

    void PushStructureEndianness(Endianness endianness);
    void PushMemberEndianness(Endianness endianness);
    void PopEndianness();

    // Position

    Task<long> GetPointerToMemberRelativeToScope(string memberPath);
    Task<long> GetSizeOfMemberRelativeToScope(string memberPath);
    void MarkStartOfMember(string memberName);
    void MarkEndOfMember();
  }
}