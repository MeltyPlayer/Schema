using System;

using schema.util.asserts;


namespace schema.binary;

public partial class SchemaBinaryReader {
  public void Subread(long len, Action subread) {
    var baseOffset = this.Position;
    this.SubreadAt(baseOffset, len, subread);
    this.Position = baseOffset + len;
  }

  public T Subread<T>(long len, Func<T> subread) {
    T value = default;
    this.Subread(len, () => { value = subread(); });
    return value!;
  }

  public void SubreadAt(long position, long len, Action subread) {
    var tempPos = this.Position;

    {
      this.MaybeAssertAlreadyAtPosition_(position);
      this.Position = position;

      var pmi = this.positionManagerImpl_;
      pmi.PushSubread(position, pmi.BaseOffset + len);
      subread();
      pmi.PopSubread();
    }

    this.Position = tempPos;
  }

  public void SubreadAt(long position, Action subread) {
    var tempPos = this.Position;
    {
      this.Position = position;
      subread();
    }
    this.Position = tempPos;
  }


  public T SubreadAt<T>(long position, long len, Func<T> subread) {
    T value = default;

    this.SubreadAt(
        position,
        len,
        () => { value = subread(); });

    return value!;
  }

  public T SubreadAt<T>(long position, Func<T> subread) {
    T value = default;

    this.SubreadAt(
        position,
        () => { value = subread(); });

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