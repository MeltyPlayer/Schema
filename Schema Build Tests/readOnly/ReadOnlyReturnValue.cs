using schema.readOnly;


namespace readOnly;

[GenerateReadOnly]
public partial class ReadOnlyReturnValue {
  [Const]
  public IFooBar Foo(int bar) => default!;
}