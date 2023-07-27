using System.IO;
using System.Threading.Tasks;

using schema.binary;
using schema.binary.attributes;
using schema.binary.testing;


namespace build {
  internal partial class SizeOfMemberInBytesTests {
    [BinarySchema]
    public partial class ParentImpl : IBinaryConvertible {
      public Child Child { get; } = new();

      public int Field { get; set; }
    }

    [BinarySchema]
    public partial class Child : IChildOf<ParentImpl>, IBinaryConvertible {
      public ParentImpl Parent { get; set; }

      [WSizeOfMemberInBytes(nameof(Parent.Field))]
      private byte fieldSize_;
    }


    [Test]
    public async Task TestSizeOfThroughParent() {
      var parent = new ParentImpl();
      parent.Field = 12;

      var ew = new EndianBinaryWriter();
      parent.Write(ew);

      var bytes = await BinarySchemaAssert.GetEndianBinaryWriterBytes(ew);
      BinarySchemaAssert.AssertSequence(
          bytes,
          new byte[] {4, 12, 0, 0, 0});
    }
  }
}