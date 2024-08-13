using System.IO;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using schema.binary;
using schema.binary.attributes;
using schema.binary.testing;


namespace build;

internal partial class IIndexedBuildTests {
  [BinarySchema]
  public partial class IndexedImpl : IBinaryConvertible, IIndexed {
    public int Index { get; set; }
    public byte Value { get; set; }
  }

  [BinarySchema]
  public partial class IndexedWrapper : IBinaryConvertible {
    [SequenceLengthSource(SchemaIntegerType.BYTE)]
    public IndexedImpl[] Field { get; set; }
  }

  [Test]
  public async Task TestReadsAsExpected() {
    using var br = new SchemaBinaryReader([3, 4, 5, 6]);

    var wrapper = br.ReadNew<IndexedWrapper>();

    CollectionAssert.AreEqual(new[] { 0, 1, 2 },
                              wrapper.Field.Select(e => e.Index));
    CollectionAssert.AreEqual(new[] { 4, 5, 6 },
                              wrapper.Field.Select(e => e.Value));
  }
}