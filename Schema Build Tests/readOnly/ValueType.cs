using schema.readOnly;


namespace readOnly {
  [GenerateReadOnly]
  public partial class ValueType {
    public string Value { get; set; }
  }

  [GenerateReadOnly]
  public partial interface IFooBar {
    public string Value { get; set; }

    [Const]
    public string Foo(int value) {
      return "";
    }
  }
}