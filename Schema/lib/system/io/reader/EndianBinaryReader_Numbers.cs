﻿using System.Runtime.CompilerServices;

using CommunityToolkit.HighPerformance;


namespace System.IO {
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
    public void AssertInt24(int expectedValue)
      => EndianBinaryReader.Assert_(expectedValue, this.ReadInt24());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadInt24() {
      Span<byte> buffer = stackalloc byte[3];
      this.BufferedStream_.FillBuffer(buffer, 3);
      return EndianBinaryReader.ConvertInt24_(buffer, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int[] ReadInt24s(long count) {
      var newArray = new int[count];
      this.ReadInt24s(newArray);
      return newArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadInt24s(int[] dst, int start, int length)
      => this.ReadInt24s(dst.AsSpan(start, length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadInt24s(Span<int> dst) {
      const int size = 3;
      this.FillBuffer_(size * dst.Length, size);
      for (var i = 0; i < dst.Length; ++i) {
        dst[i] =
            EndianBinaryReader.ConvertInt24_(this.BufferedStream_.Buffer, i);
      }
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertUInt24(uint expectedValue)
      => EndianBinaryReader.Assert_(expectedValue, this.ReadUInt24());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint ReadUInt24() {
      Span<byte> buffer = stackalloc byte[3];
      this.BufferedStream_.FillBuffer(buffer, 3);
      return EndianBinaryReader.ConvertUInt24_(buffer, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint[] ReadUInt24s(long count) {
      var newArray = new uint[count];
      this.ReadUInt24s(newArray);
      return newArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadUInt24s(uint[] dst, int start, int length)
      => this.ReadUInt24s(dst.AsSpan(start, length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadUInt24s(Span<uint> dst) {
      const int size = 3;
      this.FillBuffer_(size * dst.Length, size);
      for (var i = 0; i < dst.Length; ++i) {
        dst[i] =
            EndianBinaryReader.ConvertUInt24_(this.BufferedStream_.Buffer, i);
      }
    }


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
    public void AssertHalf(float expectedValue)
      => EndianBinaryReader.Assert_(expectedValue, this.ReadHalf());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadHalf()
      => EndianBinaryReader.ConvertHalf_(this.ReadUInt16());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float[] ReadHalfs(long count) {
      var newArray = new float[count];
      this.ReadHalfs(newArray);
      return newArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadHalfs(float[] dst, int start, int length)
      => this.ReadHalfs(dst.AsSpan(start, length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadHalfs(Span<float> dst) {
      Span<ushort> values = stackalloc ushort[dst.Length];
      this.ReadUInt16s(values);
      for (var i = 0; i < dst.Length; ++i) {
        dst[i] =
            EndianBinaryReader.ConvertHalf_(values[i]);
      }
    }



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


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertSn8(float expectedValue)
      => EndianBinaryReader.AssertAlmost_(expectedValue, this.ReadSn8());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadSn8() {
      this.FillBuffer_(sizeof(byte));
      return EndianBinaryReader.ConvertSn8_(this.BufferedStream_.Buffer, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float[] ReadSn8s(long count) {
      var newArray = new float[count];
      this.ReadSn8s(newArray);
      return newArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadSn8s(float[] dst, int start, int length)
      => this.ReadSn8s(dst.AsSpan(start, length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadSn8s(Span<float> dst) {
      const int size = sizeof(byte);
      this.FillBuffer_(size * dst.Length, size);
      for (var i = 0; i < dst.Length; ++i) {
        dst[i] =
            EndianBinaryReader.ConvertSn8_(this.BufferedStream_.Buffer, i);
      }
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertUn8(float expectedValue)
      => EndianBinaryReader.AssertAlmost_(expectedValue, this.ReadUn8());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadUn8() {
      this.FillBuffer_(sizeof(byte));
      return EndianBinaryReader.ConvertUn8_(this.BufferedStream_.Buffer, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float[] ReadUn8s(long count) {
      var newArray = new float[count];
      this.ReadUn8s(newArray);
      return newArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadUn8s(float[] dst, int start, int length)
      => this.ReadUn8s(dst.AsSpan(start, length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadUn8s(Span<float> dst) {
      const int size = sizeof(byte);
      this.FillBuffer_(size * dst.Length, size);
      for (var i = 0; i < dst.Length; ++i) {
        dst[i] =
            EndianBinaryReader.ConvertUn8_(this.BufferedStream_.Buffer, i);
      }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertSn16(float expectedValue)
      => EndianBinaryReader.AssertAlmost_(expectedValue, this.ReadSn16());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadSn16()
      => EndianBinaryReader.ConvertSn16_(this.ReadInt16());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float[] ReadSn16s(long count) {
      var newArray = new float[count];
      this.ReadSn16s(newArray);
      return newArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadSn16s(float[] dst, int start, int length)
      => this.ReadSn16s(dst.AsSpan(start, length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadSn16s(Span<float> dst) {
      Span<short> values = stackalloc short[dst.Length];
      this.ReadInt16s(values);
      for (var i = 0; i < dst.Length; ++i) {
        dst[i] =
            EndianBinaryReader.ConvertSn16_(values[i]);
      }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertUn16(float expectedValue)
      => EndianBinaryReader.AssertAlmost_(expectedValue, this.ReadUn16());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadUn16()
      => EndianBinaryReader.ConvertUn16_(this.ReadUInt16());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float[] ReadUn16s(long count) {
      var newArray = new float[count];
      this.ReadUn16s(newArray);
      return newArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadUn16s(float[] dst, int start, int length)
      => this.ReadUn16s(dst.AsSpan(start, length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadUn16s(Span<float> dst) {
      Span<ushort> values = stackalloc ushort[dst.Length];
      this.ReadUInt16s(values);
      for (var i = 0; i < dst.Length; ++i) {
        dst[i] =
            EndianBinaryReader.ConvertUn16_(values[i]);
      }
    }
  }
}