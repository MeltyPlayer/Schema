using schema.readOnly;


namespace readOnly {
  [GenerateReadOnly]
  public partial class MutableReturnValue {
    [Const]
    public IFooBar Foo(int bar) => default!;
  }
}