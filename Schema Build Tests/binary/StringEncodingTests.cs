using schema.binary.attributes;

namespace schema.binary;

public partial class StringEncodingTests {
  [BinarySchema]
  public partial class Wrapper {
    [StringLengthSource(10)]
    [StringEncoding(StringEncodingType.SHIFT_JIS)]
    public string Field { get; set; }
  }
}