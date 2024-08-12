using System;
using System.IO;

using NUnit.Framework;

using schema.binary;


namespace build {
  public partial class StructBuildTests {
    [BinarySchema]
    public partial struct SchemaStruct : IBinaryConvertible {
      public int Value { get; set; }

      public override String ToString() => this.Value.ToString();

      public override bool Equals(object other) {
        if (other is SchemaStruct otherStruct) {
          return this.Value.Equals(otherStruct.Value);
        }

        return false;
      }
    }

    [BinarySchema]
    public partial class StructWrapper : IBinaryConvertible {
      public SchemaStruct Value { get; set; }

      public override String ToString() => this.Value.ToString();

      public override bool Equals(object other) {
        if (other is StructWrapper otherWrapper) {
          return this.Value.Equals(otherWrapper.Value);
        }

        return false;
      }
    }

    [Test]
    public void TestWriteAndRead() {
      var expectedSw =
          new StructWrapper {Value = new SchemaStruct {Value = 1}};

      var ms = new MemoryStream();

      var endianness = Endianness.BigEndian;
      var ew = new SchemaBinaryWriter(endianness);

      expectedSw.Write(ew);
      ew.CompleteAndCopyTo(ms);

      ms.Position = 0;
      var er = new SchemaBinaryReader(ms, endianness);
      var actualSw = er.ReadNew<StructWrapper>();

      Assert.AreEqual(expectedSw, actualSw);
    }
  }
}