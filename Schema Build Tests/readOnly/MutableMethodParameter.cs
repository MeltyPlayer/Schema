using schema.readOnly;


namespace readOnly;

[GenerateReadOnly]
public partial interface MutableMethodParameter {
  [Const]
  public int Foo([KeepMutableType] IFooBar bar) => 0;
}