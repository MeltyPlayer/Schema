using System.Runtime.CompilerServices;


namespace schema.binary;

public sealed partial class SchemaBinaryReader {
  public long Position {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.positionManagerImpl_.Position;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set => this.positionManagerImpl_.Position = value;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AssertPosition(long expectedPosition) {
    SchemaBinaryReader.Assert_(expectedPosition, this.Position);
  }

  public long Length {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.positionManagerImpl_.Length;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void PushLocalSpace() => this.positionManagerImpl_.PushLocalSpace();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void PopLocalSpace() => this.positionManagerImpl_.PopLocalSpace();

  public bool Eof => this.bufferedStream_.Eof;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AssertNotEof() => this.bufferedStream_.AssertNotEof();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Align(uint amt) {
    var offs = amt - (this.Position % amt);
    if (offs != amt) {
      this.Position += offs;
    }
  }
}