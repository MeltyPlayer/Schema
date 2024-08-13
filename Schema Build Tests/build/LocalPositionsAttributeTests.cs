using System.Threading.Tasks;

using NUnit.Framework;

using schema.binary;
using schema.binary.attributes;
using schema.binary.testing;
using schema.binary.types.data;


namespace build;

internal partial class LocalPositionsAttributeTests {
  [BinarySchema]
  public partial class ParentImpl : IBinaryConvertible {
    public AutoUInt32SizedSection<Child> Child { get; } = new();
  }

  [BinarySchema]
  [LocalPositions]
  public partial class Child : IBinaryConvertible {
    [WPointerTo(nameof(Field))]
    private byte fieldPointer_ = 1;

    [RAtPosition(nameof(fieldPointer_))]
    public uint Field { get; set; } = 3;
  }


  [Test]
  public async Task TestPointerToThroughParent() {
    var parent = new ParentImpl();
    await BinarySchemaAssert.WritesAndReadsIdentically(
        parent,
        assertExactLength: false);
  }
}