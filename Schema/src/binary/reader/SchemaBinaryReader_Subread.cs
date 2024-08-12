using System;

using schema.util.streams;


namespace schema.binary {
  public partial class SchemaBinaryReader {
    public void Subread(int len, Action<IBinaryReader> subread) {
      var startingPosition = this.Position;

      var baseOffset = this.Position;
      var substream = new RangedReadableSubstream(this.BaseStream_,
                                                  baseOffset,
                                                  baseOffset + len);
      using var sbr = new SchemaBinaryReader(substream, this.Endianness);
      sbr.positionManagerImpl_ =
          new StreamPositionManager(substream, baseOffset);
      subread(sbr);

      this.Position = startingPosition + len;
    }

    public T Subread<T>(int len, Func<IBinaryReader, T> subread) {
      T value = default;
      this.Subread(len, sbr => { value = subread(sbr); });
      return value!;
    }

    public void SubreadAt(long position,
                          int len,
                          Action<IBinaryReader> subread) {
      var tempPos = this.Position;

      {
        this.Position = position;

        var baseOffset = this.positionManagerImpl_.BaseOffset;
        var substream =
            new RangedReadableSubstream(this.BaseStream_,
                                        position,
                                        baseOffset + len);
        using var sbr = new SchemaBinaryReader(substream, this.Endianness);
        sbr.positionManagerImpl_ =
            new StreamPositionManager(substream, baseOffset);
        subread(sbr);
      }

      this.Position = tempPos;
    }

    public void SubreadAt(long position, Action<IBinaryReader> subread) {
      var tempPos = this.Position;
      {
        this.Position = position;
        subread(this);
      }
      this.Position = tempPos;
    }


    public T SubreadAt<T>(long position,
                          int len,
                          Func<IBinaryReader, T> subread) {
      T value = default;

      this.SubreadAt(
          position,
          len,
          sbr => { value = subread(sbr); });

      return value!;
    }

    public T SubreadAt<T>(long position, Func<IBinaryReader, T> subread) {
      T value = default;

      this.SubreadAt(
          position,
          sbr => { value = subread(sbr); });

      return value!;
    }
  }
}