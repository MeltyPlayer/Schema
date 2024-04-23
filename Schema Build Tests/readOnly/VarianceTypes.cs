using System.Collections.Generic;

using schema.readOnly;


namespace readOnly {
  [GenerateReadOnly]
  public partial interface IDoesNotAddInVariance<T> {
    [Const]
    public bool TryGetSet(out ISet<T> list);
  }

  [GenerateReadOnly]
  public partial interface IOkayToAddVariance<T> {
    [Const]
    public void PassGetSet(ISet<T> list);
  }
}