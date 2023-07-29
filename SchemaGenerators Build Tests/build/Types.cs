using schema.binary;
using schema.binary.attributes;

namespace build {
  [BinarySchema]
  public partial class ClassWith1Bool : IBinaryConvertible {
    [IntegerFormat(SchemaIntegerType.INT16)]
    public bool Bool { get; private set; }
  }
}
