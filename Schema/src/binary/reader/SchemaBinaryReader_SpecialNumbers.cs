using System;
using System.Runtime.CompilerServices;


namespace schema.binary;

public sealed partial class SchemaBinaryReader {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AssertInt24(int expectedValue)
    => SchemaBinaryReader.Assert_(expectedValue, this.ReadInt24());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int ReadInt24() {
    Span<byte> buffer = stackalloc byte[3];
    this.bufferedStream_.FillBuffer(buffer, 3);
    return SchemaBinaryReader.ConvertInt24_(buffer, 0);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int[] ReadInt24s(long count) {
    var newArray = new int[count];
    this.ReadInt24s(newArray);
    return newArray;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ReadInt24s(Span<int> dst) {
    const int size = 3;
    this.FillBuffer_(size * dst.Length, size);
    for (var i = 0; i < dst.Length; ++i) {
      dst[i] =
          SchemaBinaryReader.ConvertInt24_(this.bufferedStream_.Buffer, i);
    }
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AssertUInt24(uint expectedValue)
    => SchemaBinaryReader.Assert_(expectedValue, this.ReadUInt24());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public uint ReadUInt24() {
    Span<byte> buffer = stackalloc byte[3];
    this.bufferedStream_.FillBuffer(buffer, 3);
    return SchemaBinaryReader.ConvertUInt24_(buffer, 0);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public uint[] ReadUInt24s(long count) {
    var newArray = new uint[count];
    this.ReadUInt24s(newArray);
    return newArray;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ReadUInt24s(Span<uint> dst) {
    const int size = 3;
    this.FillBuffer_(size * dst.Length, size);
    for (var i = 0; i < dst.Length; ++i) {
      dst[i] =
          SchemaBinaryReader.ConvertUInt24_(this.bufferedStream_.Buffer, i);
    }
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AssertHalf(float expectedValue)
    => SchemaBinaryReader.Assert_(expectedValue, this.ReadHalf());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float ReadHalf()
    => SchemaBinaryReader.ConvertHalf_(this.ReadUInt16());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float[] ReadHalfs(long count) {
    var newArray = new float[count];
    this.ReadHalfs(newArray);
    return newArray;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ReadHalfs(Span<float> dst) {
    Span<ushort> values = stackalloc ushort[dst.Length];
    this.ReadUInt16s(values);
    for (var i = 0; i < dst.Length; ++i) {
      dst[i] =
          SchemaBinaryReader.ConvertHalf_(values[i]);
    }
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AssertSn8(float expectedValue)
    => SchemaBinaryReader.AssertAlmost_(expectedValue, this.ReadSn8());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float ReadSn8() {
    this.FillBuffer_(sizeof(byte));
    return SchemaBinaryReader.ConvertSn8_(this.bufferedStream_.Buffer, 0);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float[] ReadSn8s(long count) {
    var newArray = new float[count];
    this.ReadSn8s(newArray);
    return newArray;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ReadSn8s(Span<float> dst) {
    const int size = sizeof(byte);
    this.FillBuffer_(size * dst.Length, size);
    for (var i = 0; i < dst.Length; ++i) {
      dst[i] =
          SchemaBinaryReader.ConvertSn8_(this.bufferedStream_.Buffer, i);
    }
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AssertUn8(float expectedValue)
    => SchemaBinaryReader.AssertAlmost_(expectedValue, this.ReadUn8());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float ReadUn8() {
    this.FillBuffer_(sizeof(byte));
    return SchemaBinaryReader.ConvertUn8_(this.bufferedStream_.Buffer, 0);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float[] ReadUn8s(long count) {
    var newArray = new float[count];
    this.ReadUn8s(newArray);
    return newArray;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ReadUn8s(Span<float> dst) {
    const int size = sizeof(byte);
    this.FillBuffer_(size * dst.Length, size);
    for (var i = 0; i < dst.Length; ++i) {
      dst[i] =
          SchemaBinaryReader.ConvertUn8_(this.bufferedStream_.Buffer, i);
    }
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AssertSn16(float expectedValue)
    => SchemaBinaryReader.AssertAlmost_(expectedValue, this.ReadSn16());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float ReadSn16()
    => SchemaBinaryReader.ConvertSn16_(this.ReadInt16());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float[] ReadSn16s(long count) {
    var newArray = new float[count];
    this.ReadSn16s(newArray);
    return newArray;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ReadSn16s(Span<float> dst) {
    Span<short> values = stackalloc short[dst.Length];
    this.ReadInt16s(values);
    for (var i = 0; i < dst.Length; ++i) {
      dst[i] =
          SchemaBinaryReader.ConvertSn16_(values[i]);
    }
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AssertUn16(float expectedValue)
    => SchemaBinaryReader.AssertAlmost_(expectedValue, this.ReadUn16());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float ReadUn16()
    => SchemaBinaryReader.ConvertUn16_(this.ReadUInt16());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float[] ReadUn16s(long count) {
    var newArray = new float[count];
    this.ReadUn16s(newArray);
    return newArray;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ReadUn16s(Span<float> dst) {
    Span<ushort> values = stackalloc ushort[dst.Length];
    this.ReadUInt16s(values);
    for (var i = 0; i < dst.Length; ++i) {
      dst[i] =
          SchemaBinaryReader.ConvertUn16_(values[i]);
    }
  }
}