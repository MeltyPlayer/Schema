using System;

using schema.util.asserts;
using schema.util.streams;


namespace schema.binary;

public partial class SchemaBinaryReader {
  public void Subread(int len, Action<IBinaryReader> subread) {
    var baseOffset = this.Position;
    this.SubreadAt(baseOffset, len, subread);
    this.Position = baseOffset + len;
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
      this.MaybeAssertAlreadyAtPosition_(position);
      this.Position = position;

      var baseOffset = this.positionManagerImpl_.BaseOffset;
      var substream =
          new RangedReadableSubstream(this.BaseStream_,
                                      position,
                                      baseOffset + len);
      using var sbr = new SchemaBinaryReader(substream, this.Endianness) {
          AssertAlreadyAtOffset = this.AssertAlreadyAtOffset,
      };
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

  private void MaybeAssertAlreadyAtPosition_(long position) {
    if (this.AssertAlreadyAtOffset) {
      if (this.Position != position) {
        Asserts.Fail(
            $"Expected position to already be {position}, but was {this.Position}");
      }
    }
  }
}