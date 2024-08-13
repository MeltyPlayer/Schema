using System;
using System.Runtime.CompilerServices;

using schema.util.streams;


namespace schema.binary;

public sealed partial class SchemaBinaryReader {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AssertByte(byte expectedValue)
    => SchemaBinaryReader.Assert_(expectedValue, this.ReadByte());


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  byte IDataReader.ReadByte() => this.ReadByte();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  byte IReadableStream.ReadByte() => this.ReadByte();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public byte ReadByte() => this.bufferedStream_.BaseStream.ReadByte();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public byte[] ReadBytes(long count) {
    var newArray = new byte[count];
    this.ReadBytes(newArray);
    return newArray;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ReadBytes(Span<byte> dst)
    => this.bufferedStream_.BaseStream.TryToReadIntoBuffer(dst);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AssertSByte(sbyte expectedValue)
    => SchemaBinaryReader.Assert_(expectedValue, this.ReadSByte());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public sbyte ReadSByte() => (sbyte) this.ReadByte();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public sbyte[] ReadSBytes(long count) {
    var newArray = new sbyte[count];
    this.ReadSBytes(newArray);
    return newArray;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ReadSBytes(Span<sbyte> dst)
    => this.bufferedStream_.FillBuffer(dst);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AssertInt16(short expectedValue)
    => SchemaBinaryReader.Assert_(expectedValue, this.ReadInt16());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public short ReadInt16() => this.bufferedStream_.Read<short>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public short[] ReadInt16s(long count) {
    var newArray = new short[count];
    this.ReadInt16s(newArray);
    return newArray;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ReadInt16s(Span<short> dst)
    => this.bufferedStream_.FillBufferAndReverse(dst);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AssertUInt16(ushort expectedValue)
    => SchemaBinaryReader.Assert_(expectedValue, this.ReadUInt16());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ushort ReadUInt16() => this.bufferedStream_.Read<ushort>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ushort[] ReadUInt16s(long count) {
    var newArray = new ushort[count];
    this.ReadUInt16s(newArray);
    return newArray;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ReadUInt16s(Span<ushort> dst)
    => this.bufferedStream_.FillBufferAndReverse(dst);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AssertInt32(int expectedValue)
    => SchemaBinaryReader.Assert_(expectedValue, this.ReadInt32());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int ReadInt32() => this.bufferedStream_.Read<int>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int[] ReadInt32s(long count) {
    var newArray = new int[count];
    this.ReadInt32s(newArray);
    return newArray;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ReadInt32s(Span<int> dst)
    => this.bufferedStream_.FillBufferAndReverse(dst);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AssertUInt32(uint expectedValue)
    => SchemaBinaryReader.Assert_(expectedValue, this.ReadUInt32());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public uint ReadUInt32() => this.bufferedStream_.Read<uint>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public uint[] ReadUInt32s(long count) {
    var newArray = new uint[count];
    this.ReadUInt32s(newArray);
    return newArray;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ReadUInt32s(Span<uint> dst)
    => this.bufferedStream_.FillBufferAndReverse(dst);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AssertInt64(long expectedValue)
    => SchemaBinaryReader.Assert_(expectedValue, this.ReadInt64());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public long ReadInt64() => this.bufferedStream_.Read<long>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public long[] ReadInt64s(long count) {
    var newArray = new long[count];
    this.ReadInt64s(newArray);
    return newArray;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ReadInt64s(Span<long> dst)
    => this.bufferedStream_.FillBufferAndReverse(dst);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AssertUInt64(ulong expectedValue)
    => SchemaBinaryReader.Assert_(expectedValue, this.ReadUInt64());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ulong ReadUInt64() => this.bufferedStream_.Read<ulong>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ulong[] ReadUInt64s(long count) {
    var newArray = new ulong[count];
    this.ReadUInt64s(newArray);
    return newArray;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ReadUInt64s(Span<ulong> dst)
    => this.bufferedStream_.FillBufferAndReverse(dst);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AssertSingle(float expectedValue)
    => SchemaBinaryReader.AssertAlmost_(expectedValue, this.ReadSingle());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float ReadSingle() => this.bufferedStream_.Read<float>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float[] ReadSingles(long count) {
    var newArray = new float[count];
    this.ReadSingles(newArray);
    return newArray;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ReadSingles(Span<float> dst)
    => this.bufferedStream_.FillBufferAndReverse(dst);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AssertDouble(double expectedValue)
    => SchemaBinaryReader.AssertAlmost_(expectedValue, this.ReadDouble());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double ReadDouble() => this.bufferedStream_.Read<double>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double[] ReadDoubles(long count) {
    var newArray = new double[count];
    this.ReadDoubles(newArray);
    return newArray;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ReadDoubles(Span<double> dst)
    => this.bufferedStream_.FillBufferAndReverse(dst);
}