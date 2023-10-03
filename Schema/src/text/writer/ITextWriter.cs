using System;

namespace schema.text.writer {
  public interface ITextWriter : ITextStream, IDataWriter, IIndentable {
    void WriteBytes(ReadOnlySpan<byte> values, string separator);
    void WriteHexByte(byte value);
    void WriteHexByte(ReadOnlySpan<byte> values, string separator);

    void WriteSBytes(ReadOnlySpan<sbyte> values, string separator);
    void WriteHexSByte(sbyte value);
    void WriteHexSBytes(ReadOnlySpan<sbyte> values, string separator);

    void WriteInt16s(ReadOnlySpan<short> values, string separator);
    void WriteHexInt16(short value);
    void WriteHexInt16s(ReadOnlySpan<short> values, string separator);

    void WriteUInt16s(ReadOnlySpan<ushort> values, string separator);
    void WriteHexUInt16(ushort value);
    void WriteHexUInt16s(ReadOnlySpan<ushort> values, string separator);

    void WriteInt32s(ReadOnlySpan<int> values, string separator);
    void WriteHexInt32(int value);
    void WriteHexInt32s(ReadOnlySpan<int> values, string separator);

    void WriteUInt32s(ReadOnlySpan<uint> values, string separator);
    void WriteHexUInt32(uint value);
    void WriteHexUInt32s(ReadOnlySpan<uint> values, string separator);

    void WriteInt64s(ReadOnlySpan<long> values, string separator);
    void WriteHexInt64(long value);
    void WriteHexInt64s(ReadOnlySpan<long> values, string separator);

    void WriteUInt64s(ReadOnlySpan<ulong> values, string separator);
    void WriteHexUInt64(ulong value);
    void WriteHexUInt64s(ReadOnlySpan<ulong> values, string separator);

    void WriteSingles(ReadOnlySpan<float> values, string separators);
    void WriteDoubles(ReadOnlySpan<double> values, string separators);

    void WriteChars(ReadOnlySpan<char> chars, string separator);
    void WriteStrings(ReadOnlySpan<string> strings, string separator);

    void Write<T>(T value) where T : ITextSerializable;

    void WriteArray<T>(ReadOnlySpan<T> values, string separator)
        where T : ITextSerializable, new();
  }
}