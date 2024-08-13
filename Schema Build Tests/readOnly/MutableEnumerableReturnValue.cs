using System.Collections.Generic;

using schema.readOnly;


namespace readOnly;

[GenerateReadOnly]
public partial class MutableEnumerableReturnValue {
  [Const]
  [KeepMutableType]
  public IEnumerable<IFooBar> Foo(int bar) => default!;
}