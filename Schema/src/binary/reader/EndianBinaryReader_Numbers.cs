using System;
using System.Runtime.CompilerServices;

using CommunityToolkit.HighPerformance;


namespace schema.binary {
  public sealed partial class EndianBinaryReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertByte(byte expectedValue)
      => EndianBinaryReader.Assert_(expectedValue, this.ReadByte());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ReadByte()
      => (byte) this.BufferedStream_.BaseStream.ReadByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] ReadBytes(long count) {
      var newArray = new byte[count];
      this.ReadBytes(newArray);
      return newArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadBytes(byte[] dst, int start, int length)
      => this.ReadBytes(dst.AsSpan(start, length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadBytes(Span<byte> dst)
      => this.BufferedStream_.BaseStream.Read(dst);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertSByte(sbyte expectedValue)
      => EndianBinaryReader.Assert_(expectedValue, this.ReadSByte());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public sbyte ReadSByte() => (sbyte) this.ReadByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public sbyte[] ReadSBytes(long count) {
      var newArray = new sbyte[count];
      this.ReadSBytes(newArray);
      return newArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadSBytes(sbyte[] dst, int start, int length)
      => this.ReadSBytes(dst.AsSpan(start, length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadSBytes(Span<sbyte> dst)
      => this.BufferedStream_.FillBuffer(dst);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertInt16(short expectedValue)
      => EndianBinaryReader.Assert_(expectedValue, this.ReadInt16());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short ReadInt16() => this.BufferedStream_.Read<short>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short[] ReadInt16s(long count) {
      var newArray = new short[count];
      this.ReadInt16s(newArray);
      return newArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadInt16s(short[] dst, int start, int length)
      => this.ReadInt16s(dst.AsSpan(start, length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadInt16s(Span<short> dst)
      => this.BufferedStream_.FillBufferAndReverse(dst);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertUInt16(ushort expectedValue)
      => EndianBinaryReader.Assert_(expectedValue, this.ReadUInt16());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort ReadUInt16() => this.BufferedStream_.Read<ushort>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort[] ReadUInt16s(long count) {
      var newArray = new ushort[count];
      this.ReadUInt16s(newArray);
      return newArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadUInt16s(ushort[] dst, int start, int length)
      => this.ReadUInt16s(dst.AsSpan(start, length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadUInt16s(Span<ushort> dst)
      => this.BufferedStream_.FillBufferAndReverse(dst);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertInt32(int expectedValue)
      => EndianBinaryReader.Assert_(expectedValue, this.ReadInt32());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadInt32() => this.BufferedStream_.Read<int>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int[] ReadInt32s(long count) {
      var newArray = new int[count];
      this.ReadInt32s(newArray);
      return newArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadInt32s(int[] dst, int start, int length)
      => this.ReadInt32s(dst.AsSpan(start, length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadInt32s(Span<int> dst)
      => this.BufferedStream_.FillBufferAndReverse(dst);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertUInt32(uint expectedValue)
      => EndianBinaryReader.Assert_(expectedValue, this.ReadUInt32());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint ReadUInt32() => this.BufferedStream_.Read<uint>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint[] ReadUInt32s(long count) {
      var newArray = new uint[count];
      this.ReadUInt32s(newArray);
      return newArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadUInt32s(uint[] dst, int start, int length)
      => this.ReadUInt32s(dst.AsSpan(start, length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadUInt32s(Span<uint> dst)
      => this.BufferedStream_.FillBufferAndReverse(dst);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertInt64(long expectedValue)
      => EndianBinaryReader.Assert_(expectedValue, this.ReadInt64());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long ReadInt64() => this.BufferedStream_.Read<long>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long[] ReadInt64s(long count) {
      var newArray = new long[count];
      this.ReadInt64s(newArray);
      return newArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadInt64s(long[] dst, int start, int length)
      => this.ReadInt64s(dst.AsSpan(start, length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadInt64s(Span<long> dst)
      => this.BufferedStream_.FillBufferAndReverse(dst);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertUInt64(ulong expectedValue)
      => EndianBinaryReader.Assert_(expectedValue, this.ReadUInt64());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong ReadUInt64() => this.BufferedStream_.Read<ulong>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong[] ReadUInt64s(long count) {
      var newArray = new ulong[count];
      this.ReadUInt64s(newArray);
      return newArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadUInt64s(ulong[] dst, int start, int length)
      => this.ReadUInt64s(dst.AsSpan(start, length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadUInt64s(Span<ulong> dst)
      => this.BufferedStream_.FillBufferAndReverse(dst);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertSingle(float expectedValue)
      => EndianBinaryReader.AssertAlmost_(expectedValue, this.ReadSingle());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadSingle() => this.BufferedStream_.Read<float>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float[] ReadSingles(long count) {
      var newArray = new float[count];
      this.ReadSingles(newArray);
      return newArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadSingles(float[] dst, int start, int length)
      => this.ReadSingles(dst.AsSpan(start, length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadSingles(Span<float> dst)
      => this.BufferedStream_.FillBufferAndReverse(dst);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertDouble(double expectedValue)
      => EndianBinaryReader.AssertAlmost_(expectedValue, this.ReadDouble());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ReadDouble() => this.BufferedStream_.Read<double>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double[] ReadDoubles(long count) {
      var newArray = new double[count];
      this.ReadDoubles(newArray);
      return newArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadDoubles(double[] dst, int start, int length)
      => this.ReadDoubles(dst.AsSpan(start, length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadDoubles(Span<double> dst)
      => this.BufferedStream_.FillBufferAndReverse(dst);
  }
}