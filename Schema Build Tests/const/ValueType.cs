using schema.@const;


namespace foo {
  [GenerateConst]
  public partial class ValueType {
    public string Value { get; set; }
  }
}