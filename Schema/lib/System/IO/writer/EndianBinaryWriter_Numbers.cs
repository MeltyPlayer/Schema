using System.Runtime.CompilerServices;

namespace System.IO {
  public sealed partial class EndianBinaryWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteByte(byte value) => this.impl_.WriteByte(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBytes(ReadOnlySpan<byte> values)
      => this.impl_.WriteBytes(values);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBytes(byte[] values, int offset, int count)
      => this.impl_.WriteBytes(values, offset, count);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSByte(sbyte value) => this.impl_.WriteAndFlip(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSBytes(ReadOnlySpan<sbyte> values)
      => this.impl_.WriteAndFlip(values);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSBytes(sbyte[] values, int offset, int count)
      => this.impl_.WriteAndFlip(values, offset, count);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt16(short value) => this.impl_.WriteAndFlip(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt16s(ReadOnlySpan<short> values)
      => this.impl_.WriteAndFlip(values);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt16s(short[] values, int offset, int count)
      => this.impl_.WriteAndFlip(values, offset, count);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt16(ushort value) => this.impl_.WriteAndFlip(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt16s(ReadOnlySpan<ushort> values)
      => this.impl_.WriteAndFlip(values);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt16s(ushort[] values, int offset, int count)
      => this.impl_.WriteAndFlip(values, offset, count);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt32(int value) => this.impl_.WriteAndFlip(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt32s(ReadOnlySpan<int> values)
      => this.impl_.WriteAndFlip(values);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt32s(int[] values, int offset, int count)
      => this.impl_.WriteAndFlip(values, offset, count);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt32(uint value) => this.impl_.WriteAndFlip(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt32s(ReadOnlySpan<uint> values)
      => this.impl_.WriteAndFlip(values);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt32s(uint[] values, int offset, int count)
      => this.impl_.WriteAndFlip(values, offset, count);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt64(long value) => this.impl_.WriteAndFlip(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt64s(ReadOnlySpan<long> values)
      => this.impl_.WriteAndFlip(values);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt64s(long[] values, int offset, int count)
      => this.impl_.WriteAndFlip(values, offset, count);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt64(ulong value) => this.impl_.WriteAndFlip(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt64s(ReadOnlySpan<ulong> values)
      => this.impl_.WriteAndFlip(values);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt64s(ulong[] values, int offset, int count)
      => this.impl_.WriteAndFlip(values, offset, count);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSingle(float value) => this.impl_.WriteAndFlip(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSingles(ReadOnlySpan<float> values)
      => this.impl_.WriteAndFlip(values);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSingles(float[] values, int offset, int count)
      => this.impl_.WriteAndFlip(values, offset, count);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteDouble(double value) => this.impl_.WriteAndFlip(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteDoubles(ReadOnlySpan<double> values)
      => this.impl_.WriteAndFlip(values);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteDoubles(double[] values, int offset, int count)
      => this.impl_.WriteAndFlip(values, offset, count);
  }
}