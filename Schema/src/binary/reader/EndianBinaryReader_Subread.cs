using System;

using schema.util.streams;

namespace schema.binary {
  public partial class EndianBinaryReader {
    public void Subread(int len, Action<IEndianBinaryReader> subread) {
      var startingPosition = this.Position;

      var baseOffset = this.Position;
      var substream = new RangedReadableSubstream(this.BaseStream_,
                                                  baseOffset,
                                                  baseOffset + len);
      using var ser = new EndianBinaryReader(substream, this.Endianness);
      ser.positionManagerImpl_ =
          new StreamPositionManager(substream, baseOffset);
      subread(ser);

      this.Position = startingPosition + len;
    }

    public T Subread<T>(int len, Func<IEndianBinaryReader, T> subread) {
      T value = default;
      this.Subread(len, ser => { value = subread(ser); });
      return value!;
    }

    public void SubreadAt(long position,
                          int len,
                          Action<IEndianBinaryReader> subread) {
      var tempPos = this.Position;

      {
        this.Position = position;

        var baseOffset = this.positionManagerImpl_.BaseOffset;
        var substream =
            new RangedReadableSubstream(this.BaseStream_,
                                        position,
                                        baseOffset + len);
        using var ser = new EndianBinaryReader(substream, this.Endianness);
        ser.positionManagerImpl_ =
            new StreamPositionManager(substream, baseOffset);
        subread(ser);
      }

      this.Position = tempPos;
    }

    public void SubreadAt(long position, Action<IEndianBinaryReader> subread) {
      var tempPos = this.Position;
      {
        this.Position = position;
        subread(this);
      }
      this.Position = tempPos;
    }


    public T SubreadAt<T>(long position,
                          int len,
                          Func<IEndianBinaryReader, T> subread) {
      T value = default;

      this.SubreadAt(
          position,
          len,
          ser => { value = subread(ser); });

      return value!;
    }

    public T SubreadAt<T>(long position, Func<IEndianBinaryReader, T> subread) {
      T value = default;

      this.SubreadAt(
          position,
          ser => { value = subread(ser); });

      return value!;
    }
  }
}