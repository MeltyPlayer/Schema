using schema.readOnly;


namespace readOnly;

[GenerateReadOnly]
public partial class MutableReturnValue {
  [Const]
  [KeepMutableType]
  public IFooBar Foo(int bar) => default!;
}