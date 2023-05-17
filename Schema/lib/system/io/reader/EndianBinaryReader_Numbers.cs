using System.Runtime.CompilerServices;


namespace System.IO {
  public sealed partial class EndianBinaryReader {
    public void AssertByte(byte expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadByte());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ReadByte()
      => (byte) this.BufferedStream_.BaseStream.ReadByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] ReadBytes(long count) => this.ReadBytes(new byte[count]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] ReadBytes(byte[] dst) {
      this.BufferedStream_.BaseStream.Read(dst, 0, dst.Length);
      return dst;
    }


    public void AssertSByte(sbyte expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadSByte());

    public sbyte ReadSByte() => (sbyte) this.ReadByte();

    public sbyte[] ReadSBytes(long count) => this.ReadSBytes(new sbyte[count]);

    public sbyte[] ReadSBytes(sbyte[] dst) {
      this.BufferedStream_.FillBuffer(dst, dst.Length);
      return dst;
    }


    public void AssertInt16(short expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadInt16());

    public short ReadInt16() => this.BufferedStream_.Read<short>();

    public short[] ReadInt16s(long count) => this.ReadInt16s(new short[count]);

    public short[] ReadInt16s(short[] dst) {
      this.BufferedStream_.FillBufferAndReverse(dst, dst.Length);
      return dst;
    }


    public void AssertUInt16(ushort expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadUInt16());

    public ushort ReadUInt16() => this.BufferedStream_.Read<ushort>();

    public ushort[] ReadUInt16s(long count)
      => this.ReadUInt16s(new ushort[count]);

    public ushort[] ReadUInt16s(ushort[] dst) {
      this.BufferedStream_.FillBufferAndReverse(dst, dst.Length);
      return dst;
    }


    public void AssertInt24(int expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadInt24());

    public int ReadInt24() {
      this.FillBuffer_(3);
      return EndianBinaryReader.ConvertInt24_(this.BufferedStream_.Buffer, 0);
    }

    public int[] ReadInt24s(long count) => this.ReadInt24s(new int[count]);

    public int[] ReadInt24s(int[] dst) {
      const int size = 3;
      this.FillBuffer_(size * dst.Length, size);
      for (var i = 0; i < dst.Length; ++i) {
        dst[i] =
            EndianBinaryReader.ConvertInt24_(this.BufferedStream_.Buffer, i);
      }

      return dst;
    }


    public void AssertUInt24(uint expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadUInt24());

    public uint ReadUInt24() {
      this.FillBuffer_(3);
      return EndianBinaryReader.ConvertUInt24_(this.BufferedStream_.Buffer, 0);
    }

    public uint[] ReadUInt24s(long count) => this.ReadUInt24s(new uint[count]);

    public uint[] ReadUInt24s(uint[] dst) {
      const int size = 3;
      this.FillBuffer_(size * dst.Length, size);
      for (var i = 0; i < dst.Length; ++i) {
        dst[i] =
            EndianBinaryReader.ConvertUInt24_(this.BufferedStream_.Buffer, i);
      }

      return dst;
    }


    public void AssertInt32(int expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadInt32());

    public int ReadInt32() => this.BufferedStream_.Read<int>();

    public int[] ReadInt32s(long count) => this.ReadInt32s(new int[count]);

    public int[] ReadInt32s(int[] dst) {
      this.BufferedStream_.FillBufferAndReverse(dst, dst.Length);
      return dst;
    }


    public void AssertUInt32(uint expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadUInt32());

    public uint ReadUInt32() => this.BufferedStream_.Read<uint>();

    public uint[] ReadUInt32s(long count) => this.ReadUInt32s(new uint[count]);

    public uint[] ReadUInt32s(uint[] dst) {
      this.BufferedStream_.FillBufferAndReverse(dst, dst.Length);
      return dst;
    }


    public void AssertInt64(long expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadInt64());

    public long ReadInt64() => this.BufferedStream_.Read<long>();

    public long[] ReadInt64s(long count) => this.ReadInt64s(new long[count]);

    public long[] ReadInt64s(long[] dst) {
      this.BufferedStream_.FillBufferAndReverse(dst, dst.Length);
      return dst;
    }


    public void AssertUInt64(ulong expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadUInt64());

    public ulong ReadUInt64() => this.BufferedStream_.Read<ulong>();

    public ulong[] ReadUInt64s(long count) =>
        this.ReadUInt64s(new ulong[count]);

    public ulong[] ReadUInt64s(ulong[] dst) {
      this.BufferedStream_.FillBufferAndReverse(dst, dst.Length);
      return dst;
    }


    public void AssertHalf(float expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadHalf());

    public float ReadHalf() {
      this.FillBuffer_(2);
      return EndianBinaryReader.ConvertHalf_(this.BufferedStream_.Buffer, 0);
    }

    public float[] ReadHalfs(long count) => this.ReadHalfs(new float[count]);

    public float[] ReadHalfs(float[] dst) {
      const int size = 2;
      this.FillBuffer_(size * dst.Length, size);
      for (var i = 0; i < dst.Length; ++i) {
        dst[i] =
            EndianBinaryReader.ConvertHalf_(this.BufferedStream_.Buffer, i);
      }

      return dst;
    }


    public void AssertSingle(float expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadSingle());

    public float ReadSingle() => this.BufferedStream_.Read<float>();

    public float[] ReadSingles(long count) =>
        this.ReadSingles(new float[count]);

    public float[] ReadSingles(float[] dst) {
      this.BufferedStream_.FillBufferAndReverse(dst, dst.Length);
      return dst;
    }


    public void AssertDouble(double expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadDouble());

    public double ReadDouble() => this.BufferedStream_.Read<double>();

    public double[] ReadDoubles(long count)
      => this.ReadDoubles(new double[count]);

    public double[] ReadDoubles(double[] dst) {
      this.BufferedStream_.FillBufferAndReverse(dst, dst.Length);
      return dst;
    }


    public void AssertSn8(float expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadSn8());

    public float ReadSn8() {
      this.FillBuffer_(sizeof(byte));
      return EndianBinaryReader.ConvertSn8_(this.BufferedStream_.Buffer, 0);
    }

    public float[] ReadSn8s(long count) => this.ReadSn8s(new float[count]);

    public float[] ReadSn8s(float[] dst) {
      const int size = sizeof(byte);
      this.FillBuffer_(size * dst.Length, size);
      for (var i = 0; i < dst.Length; ++i) {
        dst[i] =
            EndianBinaryReader.ConvertSn8_(this.BufferedStream_.Buffer, i);
      }

      return dst;
    }


    public void AssertUn8(float expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadUn8());

    public float ReadUn8() {
      this.FillBuffer_(sizeof(byte));
      return EndianBinaryReader.ConvertUn8_(this.BufferedStream_.Buffer, 0);
    }

    public float[] ReadUn8s(long count) => this.ReadUn8s(new float[count]);

    public float[] ReadUn8s(float[] dst) {
      const int size = sizeof(byte);
      this.FillBuffer_(size * dst.Length, size);
      for (var i = 0; i < dst.Length; ++i) {
        dst[i] =
            EndianBinaryReader.ConvertUn8_(this.BufferedStream_.Buffer, i);
      }

      return dst;
    }


    public void AssertSn16(float expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadSn16());

    public float ReadSn16() {
      this.FillBuffer_(sizeof(short));
      return EndianBinaryReader.ConvertSn16_(this.BufferedStream_.Buffer, 0);
    }

    public float[] ReadSn16s(long count) => this.ReadSn16s(new float[count]);

    public float[] ReadSn16s(float[] dst) {
      const int size = sizeof(short);
      this.FillBuffer_(size * dst.Length, size);
      for (var i = 0; i < dst.Length; ++i) {
        dst[i] =
            EndianBinaryReader.ConvertSn16_(this.BufferedStream_.Buffer, i);
      }

      return dst;
    }


    public void AssertUn16(float expectedValue)
      => EndianBinaryReader.Assert(expectedValue, this.ReadUn16());

    public float ReadUn16() {
      this.FillBuffer_(sizeof(ushort));
      return EndianBinaryReader.ConvertUn16_(this.BufferedStream_.Buffer, 0);
    }

    public float[] ReadUn16s(long count) => this.ReadUn16s(new float[count]);

    public float[] ReadUn16s(float[] dst) {
      const int size = sizeof(ushort);
      this.FillBuffer_(size * dst.Length, size);
      for (var i = 0; i < dst.Length; ++i) {
        dst[i] =
            EndianBinaryReader.ConvertUn16_(this.BufferedStream_.Buffer, i);
      }

      return dst;
    }
  }
}