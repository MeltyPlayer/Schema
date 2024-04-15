using schema.@const;


namespace foo {
  [GenerateConst]
  public partial class ValueType {
    public string Value { get; set; }
  }

  [GenerateConst]
  public partial interface FooBar {
    public string Value { get; set; }
  }
}