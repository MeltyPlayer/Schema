using schema.readOnly;


namespace readOnly;

[GenerateReadOnly]
public partial interface ReadOnlyMethodParameter {
  [Const]
  public int Foo(IFooBar bar) => 0;
}