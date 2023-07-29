using System.Buffers.Binary;
using System.Drawing;

using CommunityToolkit.HighPerformance;

using static schema.binary.BinarySchemaStructureParser;

namespace System.IO {
  public sealed partial class EndianBinaryWriter {
    public unsafe void WriteInt24(int value) {
      var ptr = &value;
      this.WriteInt24s(new Span<int>(ptr, 1));
    }

    public void WriteInt24s(int[] values, int offset, int count)
      => this.WriteInt24s(values.AsSpan(offset, count));

    public unsafe void WriteInt24s(ReadOnlySpan<int> values) {
      foreach (var value in values) {
        var ptr = &value;
        Span<byte> bytes = new Span<int>(ptr, 1).AsBytes().Slice(0, 3);
        this.impl_.WriteBytesAndFlip(bytes, 3);
      }
    }


    public unsafe void WriteUInt24(uint value) {
      var ptr = &value;
      this.WriteUInt24s(new Span<uint>(ptr, 1));
    }

    public void WriteUInt24s(uint[] values, int offset, int count) 
      => this.WriteUInt24s(values.AsSpan(offset, count));

    public unsafe void WriteUInt24s(ReadOnlySpan<uint> values) {
      foreach (var value in values) {
        var ptr = &value;
        Span<byte> bytes = new Span<int>(ptr, 1).AsBytes().Slice(0, 3);
        this.impl_.WriteBytesAndFlip(bytes, 3);
      }
    }


    public unsafe void WriteHalf(float value) {
      var ptr = &value;
      this.WriteHalfs(new Span<float>(ptr, 1));
    }

    public void WriteHalfs(float[] values, int offset, int count)
      => this.WriteHalfs(values.AsSpan(offset, count));

    public void WriteHalfs(ReadOnlySpan<float> values) {
      foreach (var value in values) {
        var half = new Half(value);
        this.impl_.WriteAndFlip(half);
      }
    }


    public void WriteUn8(float value) {
      var un8 = (byte) (value * 255f);
      this.WriteByte(un8);
    }

    public void WriteUn8s(float[] values, int offset, int count)
      => this.WriteUn8s(values.AsSpan());

    public void WriteUn8s(ReadOnlySpan<float> values) {
      foreach (var value in values) {
        this.WriteUn8(value);
      }
    }


    public void WriteSn8(float value) {
      var sn8 = (byte) (value * (255f / 2));
      this.WriteByte(sn8);
    }

    public void WriteSn8s(float[] values, int offset, int count)
      => this.WriteSn8s(values.AsSpan());

    public void WriteSn8s(ReadOnlySpan<float> values) {
      foreach (var value in values) {
        this.WriteSn8(value);
      }
    }


    public void WriteUn16(float value) {
      var un16 = (ushort) (value * 65535f);
      this.WriteUInt16(un16);
    }

    public void WriteUn16s(float[] values, int offset, int count)
      => this.WriteUn16s(values.AsSpan());

    public void WriteUn16s(ReadOnlySpan<float> values) {
      foreach (var value in values) {
        this.WriteUn16(value);
      }
    }


    public void WriteSn16(float value) {
      var sn16 = (short) (value * (65535f / 2));
      this.WriteInt16(sn16);
    }

    public void WriteSn16s(float[] values, int offset, int count)
      => this.WriteSn16s(values.AsSpan());

    public void WriteSn16s(ReadOnlySpan<float> values) {
      foreach (var value in values) {
        this.WriteSn16(value);
      }
    }
  }
}