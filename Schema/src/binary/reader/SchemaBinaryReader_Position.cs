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

  public bool Eof {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Position >= this.Length;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AssertNotEof() {
    if (this.Eof) {
      throw new SchemaAssertionException(
          $"Attempted to read past the end of the stream: position '{this.Position}' of stream length '{this.Length}'");
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Align(uint amt) {
    var offs = amt - (this.Position % amt);
    if (offs != amt) {
      this.Position += offs;
    }
  }
}