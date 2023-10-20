using System.IO;

using NUnit.Framework;

using schema.binary;
using schema.binary.attributes;

namespace build {
  public partial class StringLengthSourceTests {
    [BinarySchema]
    public partial class StringWrapper : IBinaryConvertible {
      [StringLengthSource(SchemaIntegerType.BYTE)]
      public string TextWithByteLength { get; set; }

      [StringLengthSource(4)]
      public string TextWithConstLength { get; set; }

      public override bool Equals(object other) {
        if (other is StringWrapper otherStringWrapper) {
          return this.TextWithByteLength.Equals(
                     otherStringWrapper.TextWithByteLength) &&
                 this.TextWithConstLength.Equals(
                     otherStringWrapper.TextWithConstLength);
        }

        return false;
      }

      public override string ToString()
        => $"{this.TextWithByteLength}, {this.TextWithConstLength}";
    }

    [Test]
    public void TestWriteAndRead() {
      var expectedSw = new StringWrapper {
          TextWithByteLength = "foobar",
          TextWithConstLength = "foob",
      };

      var ms = new MemoryStream();

      var endianness = Endianness.BigEndian;
      var ew = new SchemaBinaryWriter(endianness);

      expectedSw.Write(ew);
      ew.CompleteAndCopyToDelayed(ms).Wait();

      ms.Position = 0;
      var er = new SchemaBinaryReader(ms, endianness);
      var actualSw = er.ReadNew<StringWrapper>();

      Assert.AreEqual(expectedSw, actualSw);
    }
  }
}