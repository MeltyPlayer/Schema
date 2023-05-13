namespace schema.binary {
  [BinarySchema]
  public partial class ClassWith1Bool : IBinaryConvertible {
    [IntegerFormat(SchemaIntegerType.INT16)]
    public bool Bool { get; private set; }
  }
}
