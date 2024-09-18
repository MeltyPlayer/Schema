using System;
using System.Runtime.CompilerServices;

using CommunityToolkit.HighPerformance;


namespace schema.binary;

public sealed partial class SchemaBinaryWriter {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void WriteInt24(int value) {
    var ptr = &value;
    this.WriteInt24s(new Span<int>(ptr, 1));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void WriteInt24s(ReadOnlySpan<int> values) {
    foreach (var value in values) {
      var ptr = &value;
      Span<byte> bytes = new Span<int>(ptr, 1).AsBytes().Slice(0, 3);
      this.impl_.WriteBytesAndFlip(bytes, 3);
    }
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void WriteUInt24(uint value) {
    var ptr = &value;
    this.WriteUInt24s(new Span<uint>(ptr, 1));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void WriteUInt24s(ReadOnlySpan<uint> values) {
    foreach (var value in values) {
      var ptr = &value;
      Span<byte> bytes = new Span<int>(ptr, 1).AsBytes().Slice(0, 3);
      this.impl_.WriteBytesAndFlip(bytes, 3);
    }
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void WriteHalf(float value) {
    var ptr = &value;
    this.WriteHalfs(new Span<float>(ptr, 1));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void WriteHalfs(ReadOnlySpan<float> values) {
    foreach (var value in values) {
      var half = new Half(value);
      this.impl_.WriteAndFlip(half);
    }
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void WriteUn8(float value) {
    var un8 = (byte) (value * 255f);
    this.WriteByte(un8);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void WriteUn8s(ReadOnlySpan<float> values) {
    foreach (var value in values) {
      this.WriteUn8(value);
    }
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void WriteSn8(float value) {
    var sn8 = (byte) (value * (255f / 2));
    this.WriteByte(sn8);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void WriteSn8s(ReadOnlySpan<float> values) {
    foreach (var value in values) {
      this.WriteSn8(value);
    }
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void WriteUn16(float value) {
    var un16 = (ushort) (value * 65535f);
    this.WriteUInt16(un16);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void WriteUn16s(ReadOnlySpan<float> values) {
    foreach (var value in values) {
      this.WriteUn16(value);
    }
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void WriteSn16(float value) {
    var sn16 = (short) (value * (65535f / 2));
    this.WriteInt16(sn16);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void WriteSn16s(ReadOnlySpan<float> values) {
    foreach (var value in values) {
      this.WriteSn16(value);
    }
  }
}