using System.Runtime.CompilerServices;

using CommunityToolkit.HighPerformance;


namespace System.IO {
  public sealed partial class EndianBinaryReader {
    public void AssertByte(byte expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadByte());

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


    public void AssertSByte(sbyte expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadSByte());

    public sbyte ReadSByte() => (sbyte) this.ReadByte();

    public sbyte[] ReadSBytes(long count) {
      var newArray = new sbyte[count];
      this.ReadSBytes(newArray);
      return newArray;
    } 

    public void ReadSBytes(sbyte[] dst, int start, int length)
      => this.BufferedStream_.FillBuffer(dst, dst.Length);

    public void ReadSBytes(Span<sbyte> dst)
      => this.BufferedStream_.FillBuffer(dst);


    public void AssertInt16(short expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadInt16());

    public short ReadInt16() => this.BufferedStream_.Read<short>();

    public short[] ReadInt16s(long count) {
      var newArray = new short[count];
      this.ReadInt16s(newArray);
      return newArray;
    }

    public void ReadInt16s(short[] dst, int start, int length)
      => this.ReadInt16s(dst.AsSpan(start, length));

    public void ReadInt16s(Span<short> dst)
      => this.BufferedStream_.FillBufferAndReverse(dst);


    public void AssertUInt16(ushort expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadUInt16());

    public ushort ReadUInt16() => this.BufferedStream_.Read<ushort>();

    public ushort[] ReadUInt16s(long count) {
      var newArray = new ushort[count];
      this.ReadUInt16s(newArray);
      return newArray;
    }

    public void ReadUInt16s(ushort[] dst, int start, int length)
      => this.ReadUInt16s(dst.AsSpan(start, length));

    public void ReadUInt16s(Span<ushort> dst)
      => this.BufferedStream_.FillBufferAndReverse(dst);


    public void AssertInt24(int expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadInt24());

    public int ReadInt24() {
      this.FillBuffer_(3);
      return EndianBinaryReader.ConvertInt24_(this.BufferedStream_.Buffer, 0);
    }

    public int[] ReadInt24s(long count) {
      var newArray = new int[count];
      this.ReadInt24s(newArray);
      return newArray;
    }

    public void ReadInt24s(int[] dst, int start, int length)
      => this.ReadInt24s(dst.AsSpan(start, length));

    public void ReadInt24s(Span<int> dst) {
      const int size = 3;
      this.FillBuffer_(size * dst.Length, size);
      for (var i = 0; i < dst.Length; ++i) {
        dst[i] =
            EndianBinaryReader.ConvertInt24_(this.BufferedStream_.Buffer, i);
      }
    }



    public void AssertUInt24(uint expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadUInt24());

    public uint ReadUInt24() {
      this.FillBuffer_(3);
      return EndianBinaryReader.ConvertUInt24_(this.BufferedStream_.Buffer, 0);
    }

    public uint[] ReadUInt24s(long count) {
      var newArray = new uint[count];
      this.ReadUInt24s(newArray);
      return newArray;
    }

    public void ReadUInt24s(uint[] dst, int start, int length)
      => this.ReadUInt24s(dst.AsSpan(start, length));

    public void ReadUInt24s(Span<uint> dst) {
      const int size = 3;
      this.FillBuffer_(size * dst.Length, size);
      for (var i = 0; i < dst.Length; ++i) {
        dst[i] =
            EndianBinaryReader.ConvertUInt24_(this.BufferedStream_.Buffer, i);
      }
    }


    public void AssertInt32(int expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadInt32());

    public int ReadInt32() => this.BufferedStream_.Read<int>();

    public int[] ReadInt32s(long count) {
      var newArray = new int[count];
      this.ReadInt32s(newArray);
      return newArray;
    }

    public void ReadInt32s(int[] dst, int start, int length)
      => this.ReadInt32s(dst.AsSpan(start, length));

    public void ReadInt32s(Span<int> dst)
      => this.BufferedStream_.FillBufferAndReverse(dst);


    public void AssertUInt32(uint expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadUInt32());

    public uint ReadUInt32() => this.BufferedStream_.Read<uint>();

    public uint[] ReadUInt32s(long count) {
      var newArray = new uint[count];
      this.ReadUInt32s(newArray);
      return newArray;
    }

    public void ReadUInt32s(uint[] dst, int start, int length)
      => this.ReadUInt32s(dst.AsSpan(start, length));

    public void ReadUInt32s(Span<uint> dst)
      => this.BufferedStream_.FillBufferAndReverse(dst);


    public void AssertInt64(long expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadInt64());

    public long ReadInt64() => this.BufferedStream_.Read<long>();

    public long[] ReadInt64s(long count) {
      var newArray = new long[count];
      this.ReadInt64s(newArray);
      return newArray;
    } 

    public void ReadInt64s(long[] dst, int start, int length)
      => this.ReadInt64s(dst.AsSpan(start, length));

    public void ReadInt64s(Span<long> dst)
      => this.BufferedStream_.FillBufferAndReverse(dst);


    public void AssertUInt64(ulong expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadUInt64());

    public ulong ReadUInt64() => this.BufferedStream_.Read<ulong>();

    public ulong[] ReadUInt64s(long count) {
      var newArray = new ulong[count];
      this.ReadUInt64s(newArray);
      return newArray;
    }

    public void ReadUInt64s(ulong[] dst, int start, int length)
      => this.ReadUInt64s(dst.AsSpan(start, length));

    public void ReadUInt64s(Span<ulong> dst)
      => this.BufferedStream_.FillBufferAndReverse(dst);


    public void AssertHalf(float expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadHalf());

    public float ReadHalf() {
      this.FillBuffer_(2);
      return EndianBinaryReader.ConvertHalf_(this.BufferedStream_.Buffer, 0);
    }

    public float[] ReadHalfs(long count) {
      var newArray = new float[count];
      this.ReadHalfs(newArray);
      return newArray;
    }

    public void ReadHalfs(float[] dst, int start, int length)
      => this.ReadHalfs(dst.AsSpan(start, length));

    public void ReadHalfs(Span<float> dst) {
      const int size = 2;
      this.FillBuffer_(size * dst.Length, size);
      for (var i = 0; i < dst.Length; ++i) {
        dst[i] =
            EndianBinaryReader.ConvertHalf_(this.BufferedStream_.Buffer, i);
      }
    }



    public void AssertSingle(float expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadSingle());

    public float ReadSingle() => this.BufferedStream_.Read<float>();

    public float[] ReadSingles(long count) {
      var newArray = new float[count];
      this.ReadSingles(newArray);
      return newArray;
    }

    public void ReadSingles(float[] dst, int start, int length)
      => this.ReadSingles(dst.AsSpan(start, length));

    public void ReadSingles(Span<float> dst)
      => this.BufferedStream_.FillBufferAndReverse(dst);


    public void AssertDouble(double expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadDouble());

    public double ReadDouble() => this.BufferedStream_.Read<double>();

    public double[] ReadDoubles(long count) {
      var newArray = new double[count];
      this.ReadDoubles(newArray);
      return newArray;
    }

    public void ReadDoubles(double[] dst, int start, int length)
      => this.ReadDoubles(dst.AsSpan(start, length));

    public void ReadDoubles(Span<double> dst)
      => this.BufferedStream_.FillBufferAndReverse(dst);


    public void AssertSn8(float expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadSn8());

    public float ReadSn8() {
      this.FillBuffer_(sizeof(byte));
      return EndianBinaryReader.ConvertSn8_(this.BufferedStream_.Buffer, 0);
    }

    public float[] ReadSn8s(long count) {
      var newArray = new float[count];
      this.ReadSn8s(newArray);
      return newArray;
    }

    public void ReadSn8s(float[] dst, int start, int length)
      => this.ReadSn8s(dst.AsSpan(start, length));

    public void ReadSn8s(Span<float> dst) {
      const int size = sizeof(byte);
      this.FillBuffer_(size * dst.Length, size);
      for (var i = 0; i < dst.Length; ++i) {
        dst[i] =
            EndianBinaryReader.ConvertSn8_(this.BufferedStream_.Buffer, i);
      }
    }



    public void AssertUn8(float expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadUn8());

    public float ReadUn8() {
      this.FillBuffer_(sizeof(byte));
      return EndianBinaryReader.ConvertUn8_(this.BufferedStream_.Buffer, 0);
    }

    public float[] ReadUn8s(long count) {
      var newArray = new float[count];
      this.ReadUn8s(newArray);
      return newArray;
    }

    public void ReadUn8s(float[] dst, int start, int length)
      => this.ReadUn8s(dst.AsSpan(start, length));

    public void ReadUn8s(Span<float> dst) {
      const int size = sizeof(byte);
      this.FillBuffer_(size * dst.Length, size);
      for (var i = 0; i < dst.Length; ++i) {
        dst[i] =
            EndianBinaryReader.ConvertUn8_(this.BufferedStream_.Buffer, i);
      }
    }


    public void AssertSn16(float expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadSn16());

    public float ReadSn16() {
      this.FillBuffer_(sizeof(short));
      return EndianBinaryReader.ConvertSn16_(this.BufferedStream_.Buffer, 0);
    }

    public float[] ReadSn16s(long count) {
      var newArray = new float[count];
      this.ReadSn16s(newArray);
      return newArray;
    }

    public void ReadSn16s(float[] dst, int start, int length)
      => this.ReadSn16s(dst.AsSpan(start, length));

    public void ReadSn16s(Span<float> dst) {
      const int size = sizeof(short);
      this.FillBuffer_(size * dst.Length, size);
      for (var i = 0; i < dst.Length; ++i) {
        dst[i] =
            EndianBinaryReader.ConvertSn16_(this.BufferedStream_.Buffer, i);
      }
    }


    public void AssertUn16(float expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadUn16());

    public float ReadUn16() {
      this.FillBuffer_(sizeof(ushort));
      return EndianBinaryReader.ConvertUn16_(this.BufferedStream_.Buffer, 0);
    }

    public float[] ReadUn16s(long count) {
      var newArray = new float[count];
      this.ReadUn16s(newArray);
      return newArray;
    }

    public void ReadUn16s(float[] dst, int start, int length)
      => this.ReadUn16s(dst.AsSpan(start, length));

    public void ReadUn16s(Span<float> dst) {
      const int size = sizeof(ushort);
      this.FillBuffer_(size * dst.Length, size);
      for (var i = 0; i < dst.Length; ++i) {
        dst[i] =
            EndianBinaryReader.ConvertUn16_(this.BufferedStream_.Buffer, i);
      }
    }
  }
}