using System.IO;
using System.Threading.Tasks;

using NUnit.Framework;

using schema.binary;
using schema.binary.attributes;
using schema.binary.testing;


namespace build;

internal partial class PointerToOrNullClassTests {
  [BinarySchema]
  public partial class A : IBinaryConvertible {
    public int Value { get; set; }
  }

  [BinarySchema]
  public partial class ParentImpl : IBinaryConvertible {
    public Child Child { get; } = new();

    [RAtPositionOrNull(nameof(Child.FieldPointer), 123)]
    public A? Field { get; set; }
  }

  [BinarySchema]
  public partial class Child : IChildOf<ParentImpl>, IBinaryConvertible {
    public ParentImpl Parent { get; set; }

    [WPointerToOrNull(nameof(Parent.Field), 123)]
    public byte FieldPointer { get; set; }
  }


  [Test]
  public async Task TestReadNonnull() {
    var ms = new MemoryStream(new byte[] { 1, 12, 0, 0, 0 });
    using var br = new SchemaBinaryReader(ms);

    var parent = br.ReadNew<ParentImpl>();

    Assert.AreEqual(12, parent.Field.Value);
  }

  [Test]
  public async Task TestReadNull() {
    var ms = new MemoryStream(new byte[] { 123 });
    using var br = new SchemaBinaryReader(ms);

    var parent = br.ReadNew<ParentImpl>();

    Assert.IsNull(parent.Field);
  }


  [Test]
  public async Task TestWriteNonnull() {
    var parent = new ParentImpl();
    parent.Field = new A { Value = 12 };

    var bw = new SchemaBinaryWriter();
    parent.Write(bw);

    var bytes = await BinarySchemaAssert.GetEndianBinaryWriterBytes(bw);
    CollectionAssert.AreEqual(new byte[] { 1, 12, 0, 0, 0 }, bytes);
  }

  [Test]
  public async Task TestWriteNull() {
    var parent = new ParentImpl();
    parent.Field = null;

    var bw = new SchemaBinaryWriter();
    parent.Write(bw);

    var bytes = await BinarySchemaAssert.GetEndianBinaryWriterBytes(bw);
    CollectionAssert.AreEqual(new byte[] { 123 }, bytes);
  }
}