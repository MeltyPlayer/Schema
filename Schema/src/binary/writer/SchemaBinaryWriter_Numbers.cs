using System;
using System.Runtime.CompilerServices;


namespace schema.binary {
  public sealed partial class SchemaBinaryWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteByte(byte value) => this.impl_.WriteByte(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBytes(ReadOnlySpan<byte> values)
      => this.impl_.WriteBytes(values);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSByte(sbyte value) => this.impl_.WriteAndFlip(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSBytes(ReadOnlySpan<sbyte> values)
      => this.impl_.WriteAndFlip(values);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt16(short value) => this.impl_.WriteAndFlip(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt16s(ReadOnlySpan<short> values)
      => this.impl_.WriteAndFlip(values);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt16(ushort value) => this.impl_.WriteAndFlip(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt16s(ReadOnlySpan<ushort> values)
      => this.impl_.WriteAndFlip(values);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt32(int value) => this.impl_.WriteAndFlip(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt32s(ReadOnlySpan<int> values)
      => this.impl_.WriteAndFlip(values);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt32(uint value) => this.impl_.WriteAndFlip(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt32s(ReadOnlySpan<uint> values)
      => this.impl_.WriteAndFlip(values);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt64(long value) => this.impl_.WriteAndFlip(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt64s(ReadOnlySpan<long> values)
      => this.impl_.WriteAndFlip(values);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt64(ulong value) => this.impl_.WriteAndFlip(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt64s(ReadOnlySpan<ulong> values)
      => this.impl_.WriteAndFlip(values);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSingle(float value) => this.impl_.WriteAndFlip(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSingles(ReadOnlySpan<float> values)
      => this.impl_.WriteAndFlip(values);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteDouble(double value) => this.impl_.WriteAndFlip(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteDoubles(ReadOnlySpan<double> values)
      => this.impl_.WriteAndFlip(values);
  }
}