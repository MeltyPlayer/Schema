using schema.readOnly;


namespace foo.bar {
  [GenerateReadOnly]
  public partial interface IWrapper {
    IReadOnlyValueType Field1 { get; set; }
    IReadOnlyValueType? Field2 { get; set; }
  }
}