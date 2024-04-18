using schema.readOnly;


namespace readOnly {
  [GenerateReadOnly]
  public partial class FinMatrix<[KeepMutableType] TMutable, TReadOnly, TImpl>
      where TMutable : FinMatrix<TMutable, TReadOnly, TImpl>, TReadOnly
      where TReadOnly : IReadOnlyFinMatrix<TMutable, TReadOnly, TImpl> {
    [Const]
    public TMutable CloneAndAdd(TReadOnly other) => default!;

    [Const]
    public TMutable CloneAndAdd(in TImpl other) => default!;
  }
}