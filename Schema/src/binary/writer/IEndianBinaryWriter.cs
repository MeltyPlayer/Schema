﻿using System;
using System.IO;
using System.Threading.Tasks;

using schema.binary.attributes;

namespace schema.binary {
  public interface IEndianBinaryWriter : ISubEndianBinaryWriter, IDisposable {
    Task CompleteAndCopyToDelayed(Stream stream);
  }

  public interface ISubEndianBinaryWriter
      : IDataWriter, IEndiannessStack, ILocalSpaceStack {
    void Align(uint amt);

    void WriteBytes(ReadOnlySpan<byte> values);
    void WriteBytes(byte[] values, int offset, int count);

    void WriteSBytes(ReadOnlySpan<sbyte> values);
    void WriteSBytes(sbyte[] values, int offset, int count);

    void WriteInt16s(ReadOnlySpan<short> values);
    void WriteInt16s(short[] values, int offset, int count);

    void WriteUInt16s(ReadOnlySpan<ushort> values);
    void WriteUInt16s(ushort[] values, int offset, int count);

    void WriteInt24(int value);
    void WriteInt24s(ReadOnlySpan<int> values);
    void WriteInt24s(int[] values, int offset, int count);

    void WriteUInt24(uint value);
    void WriteUInt24s(ReadOnlySpan<uint> values);
    void WriteUInt24s(uint[] values, int offset, int count);

    void WriteInt32s(ReadOnlySpan<int> values);
    void WriteInt32s(int[] values, int offset, int count);

    void WriteUInt32s(ReadOnlySpan<uint> values);
    void WriteUInt32s(uint[] values, int offset, int count);

    void WriteInt64(long value);
    void WriteInt64s(ReadOnlySpan<long> values);
    void WriteInt64s(long[] values, int offset, int count);

    void WriteUInt64(ulong value);
    void WriteUInt64s(ReadOnlySpan<ulong> values);
    void WriteUInt64s(ulong[] values, int offset, int count);

    void WriteHalf(float value);
    void WriteHalfs(ReadOnlySpan<float> values);
    void WriteHalfs(float[] values, int offset, int count);

    void WriteSingles(ReadOnlySpan<float> values);
    void WriteSingles(float[] values, int offset, int count);

    void WriteDoubles(ReadOnlySpan<double> values);
    void WriteDoubles(double[] values, int offset, int count);

    void WriteUn8(float value);
    void WriteUn8s(ReadOnlySpan<float> values);
    void WriteUn8s(float[] values, int offset, int count);

    void WriteSn8(float value);
    void WriteSn8s(ReadOnlySpan<float> values);
    void WriteSn8s(float[] values, int offset, int count);

    void WriteUn16(float value);
    void WriteUn16s(ReadOnlySpan<float> values);
    void WriteUn16s(float[] values, int offset, int count);

    void WriteSn16(float value);
    void WriteSn16s(ReadOnlySpan<float> values);
    void WriteSn16s(float[] values, int offset, int count);

    void WriteChar(StringEncodingType encodingType, char value);
    void WriteChars(StringEncodingType encodingType, ReadOnlySpan<char> values);

    void WriteChars(StringEncodingType encodingType,
                    char[] values,
                    int offset,
                    int count);

    void WriteStringNT(string value);

    void WriteString(StringEncodingType encodingType, string value);
    void WriteStringNT(StringEncodingType encodingType, string value);

    void WriteStringWithExactLength(string value, int length);

    void WriteStringWithExactLength(StringEncodingType encodingType,
                                    string value,
                                    int length);

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


    // Position

    Task<long> GetPointerToMemberRelativeToScope(string memberPath);
    Task<long> GetSizeOfMemberRelativeToScope(string memberPath);
    void MarkStartOfMember(string memberName);
    void MarkEndOfMember();
  }
}