using schema.readOnly;


namespace readOnly {
  [GenerateReadOnly]
  public partial interface IFinMatrix<[UseMutableTypeInReadOnly] TMutable, TReadOnly, TImpl>
      where TMutable : IFinMatrix<TMutable, TReadOnly, TImpl>, TReadOnly
      where TReadOnly : IReadOnlyFinMatrix<TMutable, TReadOnly, TImpl> {
    [Const]
    TMutable CloneAndAdd(TReadOnly other);

    [Const]
    TMutable CloneAndAdd(in TImpl other);
  }
}