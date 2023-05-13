// Decompiled with JetBrains decompiler
// Type: System.IO.EndianBinaryWriter
// Assembly: MKDS Course Modifier, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DAEF8B62-698B-42D0-BEDD-3770EB8C9FE8
// Assembly location: R:\Documents\CSharpWorkspace\Pikmin2Utility\MKDS Course Modifier\MKDS Course Modifier.exe

using System.Linq;
using System.Text;

using schema.binary.io;


namespace System.IO {
  public sealed partial class EndianBinaryWriter : IEndianBinaryWriter {
    private readonly IDelayedContentOutputStream impl_;

    private bool disposed_;
    private byte[] buffer_;

    public EndianBinaryWriter() {
      this.endiannessImpl_ = new EndiannessStackImpl(null);
      this.impl_ = new DelayedContentOutputStream();
    }

    public EndianBinaryWriter(Endianness endianness) {
      this.endiannessImpl_ = new EndiannessStackImpl(endianness);
      this.impl_ = new DelayedContentOutputStream();
    }

    private EndianBinaryWriter(Endianness? endianness, 
                               ISubDelayedContentOutputStream impl) {
      this.endiannessImpl_ = new EndiannessStackImpl(endianness);
      this.impl_ = impl as IDelayedContentOutputStream;
    }

    ~EndianBinaryWriter() {
      this.Dispose(false);
    }

    public void Align(uint amt) => this.impl_.Align(amt);

    private void WriteBuffer_(int bytes, int stride) {
      if (this.IsOppositeEndiannessOfSystem) {
        for (int index = 0; index < bytes; index += stride)
          Array.Reverse((Array) this.buffer_, index, stride);
      }
      this.impl_.Write(this.buffer_, 0, bytes);
    }

    private void CreateBuffer_(int size) {
      if (this.buffer_ != null && this.buffer_.Length >= size)
        return;
      this.buffer_ = new byte[size];
    }

    public void WriteByte(byte value) {
      this.CreateBuffer_(1);
      this.buffer_[0] = value;
      this.WriteBuffer_(1, 1);
    }

    public void WriteBytes(byte[] value) =>
        this.WriteBytes(value, 0, value.Length);

    public void WriteBytes(byte[] value, int offset, int count) {
      this.CreateBuffer_(count);
      Array.Copy((Array) value, offset, (Array) this.buffer_, 0, count);
      this.WriteBuffer_(count, 1);
    }

    public void WriteSByte(sbyte value) {
      this.CreateBuffer_(1);
      this.buffer_[0] = (byte) value;
      this.WriteBuffer_(1, 1);
    }

    public void WriteSBytes(sbyte[] value) =>
        this.WriteSBytes(value, 0, value.Length);

    public void WriteSBytes(sbyte[] value, int offset, int count) {
      this.CreateBuffer_(count);
      for (int index = 0; index < count; ++index)
        this.buffer_[index] = (byte) value[index + offset];
      this.WriteBuffer_(count, 1);
    }

    public void WriteChar(char value) => this.WriteChar(value, Encoding.ASCII);

    public void WriteChar(char value, Encoding encoding) {
      int encodingSize = EndianBinaryWriter.GetEncodingSize_(encoding);
      this.CreateBuffer_(encodingSize);
      Array.Copy((Array) encoding.GetBytes(new string(value, 1)),
                 0,
                 (Array) this.buffer_,
                 0,
                 encodingSize);
      this.WriteBuffer_(encodingSize, encodingSize);
    }

    public void WriteChars(char[] value) =>
        this.WriteChars(value, 0, value.Length, Encoding.ASCII);

    public void WriteChars(char[] value,
                           int offset,
                           int count,
                           Encoding encoding) {
      int encodingSize = EndianBinaryWriter.GetEncodingSize_(encoding);
      this.CreateBuffer_(encodingSize * count);
      Array.Copy((Array) encoding.GetBytes(value, offset, count),
                 0,
                 (Array) this.buffer_,
                 0,
                 count * encodingSize);
      this.WriteBuffer_(encodingSize * count, encodingSize);
    }

    private static int GetEncodingSize_(Encoding encoding) {
      return encoding == Encoding.UTF8 ||
             encoding == Encoding.ASCII ||
             encoding != Encoding.Unicode &&
             encoding != Encoding.BigEndianUnicode
                 ? 1
                 : 2;
    }

    public void WriteString(string value)
      => this.WriteString(value, Encoding.ASCII, false);

    public void WriteStringNT(string value)
      => this.WriteString(value, Encoding.ASCII, true);

    public void WriteString(string value,
                            Encoding encoding,
                            bool nullTerminated) {
      this.WriteChars(value.ToCharArray(), 0, value.Length, encoding);
      if (!nullTerminated)
        return;
      this.WriteChar(char.MinValue, encoding);
    }


    public void WriteStringWithExactLength(string value, int length) {
      var difference = length - value.Length;
      if (difference < 0) {
        value = value.Substring(0, length);
      } else if (difference > 0) {
        var extender = new StringBuilder();
        extender.Append(value);
        for (var i = 0; i < difference; ++i) {
          extender.Append('\0');
        }
        value = extender.ToString();
      }
      this.WriteString(value);
    }


    public void WriteStringEndian(string value)
      => this.WriteStringEndian(
          value,
          Encoding.ASCII);

    public void WriteStringEndian(string value, Encoding encoding)
      => this.WriteString(
          this.IsOppositeEndiannessOfSystem ? new string(value.Reverse().ToArray()) : value,
          encoding,
          false);


    public void WriteDouble(double value) {
      this.CreateBuffer_(8);
      Array.Copy((Array) BitConverter.GetBytes(value),
                 0,
                 (Array) this.buffer_,
                 0,
                 8);
      this.WriteBuffer_(8, 8);
    }

    public void WriteDoubles(double[] value) =>
        this.WriteDoubles(value, 0, value.Length);

    public void WriteDoubles(double[] value, int offset, int count) {
      this.CreateBuffer_(8 * count);
      for (int index = 0; index < count; ++index)
        Array.Copy((Array) BitConverter.GetBytes(value[index + offset]),
                   0,
                   (Array) this.buffer_,
                   index * 8,
                   8);
      this.WriteBuffer_(8 * count, 8);
    }


    private static byte[] GetHalfBytes_(float value) {
      var half = new Half(value);
      return Half.GetBytes(half);
    }

    public void WriteHalf(float value) {
      this.CreateBuffer_(2);
      Array.Copy(GetHalfBytes_(value),
                 0,
                 this.buffer_,
                 0,
                 2);
      this.WriteBuffer_(2, 2);
    }

    public void WriteHalfs(float[] value)
      => this.WriteSingles(value, 0, value.Length);

    public void WriteHalfs(float[] value, int offset, int count) {
      this.CreateBuffer_(2 * count);
      for (int index = 0; index < count; ++index)
        Array.Copy((Array) BitConverter.GetBytes(value[index + offset]),
                   0,
                   (Array) this.buffer_,
                   index * 2,
                   2);
      this.WriteBuffer_(2 * count, 2);
    }


    public void WriteSingle(float value) {
      this.CreateBuffer_(4);
      Array.Copy((Array) BitConverter.GetBytes(value),
                 0,
                 (Array) this.buffer_,
                 0,
                 4);
      this.WriteBuffer_(4, 4);
    }

    public void WriteSingles(float[] value) =>
        this.WriteSingles(value, 0, value.Length);

    public void WriteSingles(float[] value, int offset, int count) {
      this.CreateBuffer_(4 * count);
      for (int index = 0; index < count; ++index)
        Array.Copy((Array) BitConverter.GetBytes(value[index + offset]),
                   0,
                   (Array) this.buffer_,
                   index * 4,
                   4);
      this.WriteBuffer_(4 * count, 4);
    }

    public void WriteInt24(int value) {
      this.CreateBuffer_(3);
      Array.Copy((Array)BitConverter.GetBytes(value),
                 0,
                 (Array)this.buffer_,
                 0,
                 3);
      this.WriteBuffer_(3, 3);
    }

    public void WriteInt24s(int[] value) =>
        this.WriteInt24s(value, 0, value.Length);

    public void WriteInt24s(int[] value, int offset, int count) {
      this.CreateBuffer_(3 * count);
      for (int index = 0; index < count; ++index)
        Array.Copy((Array)BitConverter.GetBytes(value[index + offset]),
                   0,
                   (Array)this.buffer_,
                   index * 3,
                   3);
      this.WriteBuffer_(3 * count, 3);
    }

    public void WriteInt32(int value) {
      this.CreateBuffer_(4);
      Array.Copy((Array) BitConverter.GetBytes(value),
                 0,
                 (Array) this.buffer_,
                 0,
                 4);
      this.WriteBuffer_(4, 4);
    }

    public void WriteInt32s(int[] value) =>
        this.WriteInt32s(value, 0, value.Length);

    public void WriteInt32s(int[] value, int offset, int count) {
      this.CreateBuffer_(4 * count);
      for (int index = 0; index < count; ++index)
        Array.Copy((Array) BitConverter.GetBytes(value[index + offset]),
                   0,
                   (Array) this.buffer_,
                   index * 4,
                   4);
      this.WriteBuffer_(4 * count, 4);
    }

    public void WriteInt64(long value) {
      this.CreateBuffer_(8);
      Array.Copy((Array) BitConverter.GetBytes(value),
                 0,
                 (Array) this.buffer_,
                 0,
                 8);
      this.WriteBuffer_(8, 8);
    }

    public void WriteInt64s(long[] value) =>
        this.WriteInt64s(value, 0, value.Length);

    public void WriteInt64s(long[] value, int offset, int count) {
      this.CreateBuffer_(8 * count);
      for (int index = 0; index < count; ++index)
        Array.Copy((Array) BitConverter.GetBytes(value[index + offset]),
                   0,
                   (Array) this.buffer_,
                   index * 8,
                   8);
      this.WriteBuffer_(8 * count, 8);
    }

    public void WriteInt16(short value) {
      this.CreateBuffer_(2);
      Array.Copy((Array) BitConverter.GetBytes(value),
                 0,
                 (Array) this.buffer_,
                 0,
                 2);
      this.WriteBuffer_(2, 2);
    }

    public void WriteInt16s(short[] value) =>
        this.WriteInt16s(value, 0, value.Length);

    public void WriteInt16s(short[] value, int offset, int count) {
      this.CreateBuffer_(2 * count);
      for (int index = 0; index < count; ++index)
        Array.Copy((Array) BitConverter.GetBytes(value[index + offset]),
                   0,
                   (Array) this.buffer_,
                   index * 2,
                   2);
      this.WriteBuffer_(2 * count, 2);
    }

    public void WriteUInt16(ushort value) {
      this.CreateBuffer_(2);
      Array.Copy((Array) BitConverter.GetBytes(value),
                 0,
                 (Array) this.buffer_,
                 0,
                 2);
      this.WriteBuffer_(2, 2);
    }

    public void WriteUInt16s(ushort[] value) =>
        this.WriteUInt16s(value, 0, value.Length);

    public void WriteUInt16s(ushort[] value, int offset, int count) {
      this.CreateBuffer_(2 * count);
      for (int index = 0; index < count; ++index)
        Array.Copy((Array) BitConverter.GetBytes(value[index + offset]),
                   0,
                   (Array) this.buffer_,
                   index * 2,
                   2);
      this.WriteBuffer_(2 * count, 2);
    }

    public void WriteUInt24(uint value) {
      this.CreateBuffer_(3);
      Array.Copy((Array)BitConverter.GetBytes(value),
                 0,
                 (Array)this.buffer_,
                 0,
                 3);
      this.WriteBuffer_(3, 3);
    }

    public void WriteUInt24s(uint[] value) =>
        this.WriteUInt24s(value, 0, value.Length);

    public void WriteUInt24s(uint[] value, int offset, int count) {
      this.CreateBuffer_(3 * count);
      for (int index = 0; index < count; ++index)
        Array.Copy((Array)BitConverter.GetBytes(value[index + offset]),
                   0,
                   (Array)this.buffer_,
                   index * 3,
                   3);
      this.WriteBuffer_(3 * count, 3);
    }

    public void WriteUInt32(uint value) {
      this.CreateBuffer_(4);
      Array.Copy((Array) BitConverter.GetBytes(value),
                 0,
                 (Array) this.buffer_,
                 0,
                 4);
      this.WriteBuffer_(4, 4);
    }

    public void WriteUInt32s(uint[] value) =>
        this.WriteUInt32s(value, 0, value.Length);

    public void WriteUInt32s(uint[] value, int offset, int count) {
      this.CreateBuffer_(4 * count);
      for (int index = 0; index < count; ++index)
        Array.Copy((Array) BitConverter.GetBytes(value[index + offset]),
                   0,
                   (Array) this.buffer_,
                   index * 4,
                   4);
      this.WriteBuffer_(4 * count, 4);
    }

    public void WriteUInt64(ulong value) {
      this.CreateBuffer_(8);
      Array.Copy((Array) BitConverter.GetBytes(value),
                 0,
                 (Array) this.buffer_,
                 0,
                 8);
      this.WriteBuffer_(8, 8);
    }

    public void WriteUInt64s(ulong[] value) =>
        this.WriteUInt64s(value, 0, value.Length);

    public void WriteUInt64s(ulong[] value, int offset, int count) {
      this.CreateBuffer_(8 * count);
      for (int index = 0; index < count; ++index)
        Array.Copy((Array) BitConverter.GetBytes(value[index + offset]),
                   0,
                   (Array) this.buffer_,
                   index * 8,
                   8);
      this.WriteBuffer_(8 * count, 8);
    }


    public void WriteUn8(float value) {
      var un8 = (byte) (value * 255f);
      this.WriteByte(un8);
    }

    public void WriteUn8s(float[] value) =>
        this.WriteUn8s(value, 0, value.Length);

    public void WriteUn8s(float[] value, int offset, int count) {
      for (var i = 0; i < count; ++i) {
        this.WriteUn8(value[offset + i]);
      }
    }


    public void WriteSn8(float value) {
      var sn8 = (byte) (value * (255f / 2));
      this.WriteByte(sn8);
    }

    public void WriteSn8s(float[] value) =>
        this.WriteSn8s(value, 0, value.Length);

    public void WriteSn8s(float[] value, int offset, int count) {
      for (var i = 0; i < count; ++i) {
        this.WriteSn8(value[offset + i]);
      }
    }


    public void WriteUn16(float value) {
      var un16 = (ushort) (value * 65535f);
      this.WriteUInt16(un16);
    }

    public void WriteUn16s(float[] value) =>
        this.WriteUn16s(value, 0, value.Length);

    public void WriteUn16s(float[] value, int offset, int count) {
      for (var i = 0; i < count; ++i) {
        this.WriteUn16(value[offset + i]);
      }
    }


    public void WriteSn16(float value) {
      var sn16 = (short) (value * (65535f / 2));
      this.WriteInt16(sn16);
    }

    public void WriteSn16s(float[] value) =>
        this.WriteSn16s(value, 0, value.Length);

    public void WriteSn16s(float[] value, int offset, int count) {
      for (var i = 0; i < count; ++i) {
        this.WriteSn16(value[offset + i]);
      }
    }


    public void Close() {
      this.Dispose();
    }

    public void Dispose() {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    private void Dispose(bool disposing) {
      if (this.disposed_)
        return;
      this.buffer_ = (byte[]) null;
      this.disposed_ = true;
    }
  }
}