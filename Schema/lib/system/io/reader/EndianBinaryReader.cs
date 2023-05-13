// Decompiled with JetBrains decompiler
// Type: System.IO.EndianBinaryReader
// Assembly: MKDS Course Modifier, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DAEF8B62-698B-42D0-BEDD-3770EB8C9FE8
// Assembly location: R:\Documents\CSharpWorkspace\Pikmin2Utility\MKDS Course Modifier\MKDS Course Modifier.exe

using System.Runtime.CompilerServices;

using schema.binary;
using SubstreamSharp;


namespace System.IO {
  public sealed partial class EndianBinaryReader : IEndianBinaryReader {
    private bool disposed_;

    // TODO: This should be private.
    // TODO: Does caching the buffer actually help, or can this logic be pulled into the extensions?
    private EndianBinaryBufferedStream BufferedStream_ { get; set; }

    private Stream BaseStream_ {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this.BufferedStream_.BaseStream;
    }

    public EndianBinaryReader(byte[] data)
      => this.Init_(new MemoryStream(data), null);

    public EndianBinaryReader(byte[] data, Endianness endianness)
      => this.Init_(new MemoryStream(data), endianness);

    public EndianBinaryReader(Stream baseStream)
      => this.Init_(baseStream, null);

    public EndianBinaryReader(Stream baseStream, Endianness endianness)
      => this.Init_(baseStream, endianness);

    private void Init_(Stream baseStream, Endianness? endianness) {
      if (baseStream == null) {
        throw new ArgumentNullException(nameof(baseStream));
      }

      if (!baseStream.CanRead) {
        throw new ArgumentException(nameof(baseStream));
      }

      this.BufferedStream_ = new EndianBinaryBufferedStream(endianness) {
          BaseStream = baseStream,
      };
    }

    ~EndianBinaryReader() {
      this.Dispose(false);
    }

    public void Close() => Dispose();

    public void Dispose() {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    private void Dispose(bool disposing) {
      if (this.disposed_) {
        return;
      }

      if (disposing && this.BaseStream_ != null) {
        this.BaseStream_.Close();
      }

      this.disposed_ = true;
    }

    public long Position {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this.BaseStream_.Position;
      set => this.BaseStream_.Position = value;
    }

    public void AssertPosition(long expectedPosition) {
      EndianBinaryReader.Assert(expectedPosition, this.Position);
    }

    public long Length {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this.BaseStream_.Length;
    }

    public bool Eof {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this.Position >= this.Length;
    }

    public void AssertNotEof() {
      if (this.Eof) {
        throw new Exception(
            $"Attempted to read past the end of the stream: position '{this.Position}' of stream length '{this.Length}'");
      }
    }

    public void Align(uint amt) {
      var offs = amt - (this.Position % amt);
      if (offs != amt) {
        this.Position += offs;
      }
    }

    public byte[] ReadBytesAtOffset(long position, int len) {
      var startingOffset = this.Position;
      this.Position = position;

      var bytes = this.ReadBytes(len);

      this.Position = startingOffset;

      return bytes;
    }

    public string ReadStringAtOffset(long position, int len) {
      var startingOffset = this.Position;
      this.Position = position;

      var str = this.ReadString(len);

      this.Position = startingOffset;

      return str;
    }

    public string ReadStringNTAtOffset(long position) {
      var startingOffset = this.Position;
      this.Position = position;

      var str = this.ReadStringNT();

      this.Position = startingOffset;

      return str;
    }

    public T ReadNewAtOffset<T>(long position)
        where T : IBinaryDeserializable, new() {
      var startingOffset = this.Position;
      this.Position = position;

      var value = this.ReadNew<T>();

      this.Position = startingOffset;

      return value;
    }

    public T[] ReadNewArrayAtOffset<T>(long position, int length)
        where T : IBinaryDeserializable, new() {
      var startingOffset = this.Position;
      this.Position = position;

      var values = this.ReadNewArray<T>(length);

      this.Position = startingOffset;

      return values;
    }



    public void Subread(long position,
                        int len,
                        Action<IEndianBinaryReader> subread) {
      var tempPos = this.Position;
      {
        this.Position = position;

        using var ser =
            new EndianBinaryReader(
                new Substream(this.BaseStream_, position, len),
                this.Endianness);
        subread(ser);
      }
      this.Position = tempPos;
    }

    public void Subread(long position, Action<IEndianBinaryReader> subread) {
      var tempPos = this.Position;
      {
        this.Position = position;
        subread(this);
      }
      this.Position = tempPos;
    }


    private static void Assert<T>(T expectedValue, T actualValue) {
      if (!expectedValue.Equals(actualValue)) {
        throw new Exception(
            "Expected " + actualValue + " to be " + expectedValue);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FillBuffer_(long count, int? optStride = null)
      => this.BufferedStream_.FillBuffer(count, optStride);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FillBuffer_(Span<byte> buffer)
      => this.BufferedStream_.FillBuffer(buffer);


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

    public T ReadNew<T>() where T : IBinaryDeserializable, new() {
      this.AssertNotEof();
      var value = new T();
      value.Read(this);
      return value;
    }

    public bool TryReadNew<T>(out T? value)
        where T : IBinaryDeserializable, new() {
      var originalPosition = this.Position;
      try {
        value = this.ReadNew<T>();
        return true;
      } catch {
        this.Position = originalPosition;
        value = default;
        return false;
      }
    }

    public void ReadNewArray<T>(out T[] array, int length)
        where T : IBinaryDeserializable, new() {
      array = ReadNewArray<T>(length);
    }

    public T[] ReadNewArray<T>(int length)
        where T : IBinaryDeserializable, new() {
      var array = new T[length];
      for (var i = 0; i < length; ++i) {
        this.AssertNotEof();
        array[i] = this.ReadNew<T>();
      }
      return array;
    }
  }
}