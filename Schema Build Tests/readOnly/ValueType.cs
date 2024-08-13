using schema.readOnly;


namespace readOnly;

[GenerateReadOnly]
public partial interface IFooBar {
  [Const]
  public SomeType<IValueType> Method(SomeType<IValueType> value) {
    return value;
  }

  public SomeType<IValueType> Property { get; set; }

  public SomeType<IValueType> this[SomeType<IValueType> foo] { get; set; }
}