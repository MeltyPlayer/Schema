using System;
using System.IO;
using System.Threading.Tasks;

using schema.binary.attributes;
using schema.util.streams;


namespace schema.binary {
  public interface ITopLevelBinaryWriter : IBinaryWriter, IDisposable {
    void CompleteAndCopyTo(Stream stream);
    void CompleteAndCopyTo(ISizedWritableStream stream);

    Task CompleteAndCopyToAsync(Stream stream);
    Task CompleteAndCopyToAsync(ISizedWritableStream stream);
  }

  public interface IBinaryWriter
      : IDataWriter, IEndiannessStack, ILocalSpaceStack {
    void Align(uint amt);

    void WriteBytes(ReadOnlySpan<byte> values);

    void WriteSBytes(ReadOnlySpan<sbyte> values);

    void WriteInt16s(ReadOnlySpan<short> values);

    void WriteUInt16s(ReadOnlySpan<ushort> values);

    void WriteInt24(int value);
    void WriteInt24s(ReadOnlySpan<int> values);

    void WriteUInt24(uint value);
    void WriteUInt24s(ReadOnlySpan<uint> values);

    void WriteInt32s(ReadOnlySpan<int> values);

    void WriteUInt32s(ReadOnlySpan<uint> values);

    void WriteInt64(long value);
    void WriteInt64s(ReadOnlySpan<long> values);

    void WriteUInt64(ulong value);
    void WriteUInt64s(ReadOnlySpan<ulong> values);

    void WriteHalf(float value);
    void WriteHalfs(ReadOnlySpan<float> values);

    void WriteSingles(ReadOnlySpan<float> values);

    void WriteDoubles(ReadOnlySpan<double> values);

    void WriteUn8(float value);
    void WriteUn8s(ReadOnlySpan<float> values);

    void WriteSn8(float value);
    void WriteSn8s(ReadOnlySpan<float> values);

    void WriteUn16(float value);
    void WriteUn16s(ReadOnlySpan<float> values);

    void WriteSn16(float value);
    void WriteSn16s(ReadOnlySpan<float> values);

    void WriteChar(StringEncodingType encodingType, char value);
    void WriteChars(StringEncodingType encodingType, ReadOnlySpan<char> values);

    void WriteStringNT(string value);

    void WriteString(StringEncodingType encodingType, string value);
    void WriteStringNT(StringEncodingType encodingType, string value);

    void WriteStringWithExactLength(string value, int length);

    void WriteStringWithExactLength(StringEncodingType encodingType,
                                    string value,
                                    int length);

    void Close();


    // Delayed

    IBinaryWriter EnterBlock(out Task<long> delayedLength);

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


    // Position

    Task<long> GetPointerToMemberRelativeToScope(string memberPath);
    Task<long> GetSizeOfMemberRelativeToScope(string memberPath);
    void MarkStartOfMember(string memberName);
    void MarkEndOfMember();
  }
}