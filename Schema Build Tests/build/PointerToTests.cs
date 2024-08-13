using System.Threading.Tasks;

using NUnit.Framework;

using schema.binary;
using schema.binary.attributes;
using schema.binary.testing;


namespace build;

internal partial class PointerToTests {
  [BinarySchema]
  public partial class ParentImpl : IBinaryConvertible {
    public Child Child { get; } = new();

    public int Field { get; set; }
  }

  [BinarySchema]
  public partial class Child : IChildOf<ParentImpl>, IBinaryConvertible {
    public ParentImpl Parent { get; set; }

    [WPointerTo(nameof(Parent.Field))]
    private byte fieldPointer_;
  }


  [Test]
  public async Task TestPointerToThroughParent() {
      var parent = new ParentImpl();
      parent.Field = 12;

      var bw = new SchemaBinaryWriter();
      parent.Write(bw);

      var bytes = await BinarySchemaAssert.GetEndianBinaryWriterBytes(bw);
      CollectionAssert.AreEqual(new byte[] {1, 12, 0, 0, 0}, bytes);
    }
}