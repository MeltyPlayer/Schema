using System.IO;
using System.Linq;

using NUnit.Framework;

using schema.binary.attributes.sequence;

namespace schema.binary.build {
  public partial class SequenceBuildTests {
    [BinarySchema]
    public partial class SequenceWrapper : IBinaryConvertible {
      [SequenceLengthSource(SchemaIntegerType.BYTE)]
      public int[] Values { get; set; }

      public override bool Equals(object other) {
        if (other is SequenceWrapper otherSequenceWrapper) {
          return this.Values.SequenceEqual(otherSequenceWrapper.Values);
        }

        return false;
      }
    }

    [Test]
    public void TestWriteAndRead() {
      var expectedSw = new SequenceWrapper {
          Values = new[] { 1, 2, 3, 4, 5, 9, 8, 7, 6 }
      };

      var ms = new MemoryStream();

      var endianness = Endianness.BigEndian;
      var ew = new EndianBinaryWriter(endianness);

      expectedSw.Write(ew);
      ew.CompleteAndCopyToDelayed(ms).Wait();

      ms.Position = 0;
      var er = new EndianBinaryReader(ms, endianness);
      var actualSw = er.ReadNew<SequenceWrapper>();

      Assert.AreEqual(expectedSw, actualSw);
    }
  }
}