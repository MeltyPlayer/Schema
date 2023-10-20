using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

using schema.binary;
using schema.binary.attributes;

namespace build {
  public partial class StructSequenceTests {
    [BinarySchema]
    public partial struct SchemaStruct : IBinaryConvertible {
      public int Value { get; set; }

      public override bool Equals(object other) {
        if (other is SchemaStruct otherStruct) {
          return this.Value.Equals(otherStruct.Value);
        }

        return false;
      }
    }


    [BinarySchema]
    public partial class StructArraySequenceWrapper : IBinaryConvertible {
      [SequenceLengthSource(SchemaIntegerType.BYTE)]
      public SchemaStruct[] Values { get; set; }

      public override bool Equals(object other) {
        if (other is StructArraySequenceWrapper otherSequenceWrapper) {
          return this.Values.SequenceEqual(otherSequenceWrapper.Values);
        }

        return false;
      }
    }

    [Test]
    public void TestWriteAndReadArrayObject() {
      var expectedSw = new StructArraySequenceWrapper {
          Values = new[] {
              new SchemaStruct { Value = 1 },
              new SchemaStruct { Value = 2 },
              new SchemaStruct { Value = 3 }
          }
      };

      var ms = new MemoryStream();

      var endianness = Endianness.BigEndian;

      var ew = new SchemaBinaryWriter(endianness);
      expectedSw.Write(ew);
      ew.CompleteAndCopyToDelayed(ms).Wait();

      var er = new SchemaBinaryReader(ms, endianness);
      er.Position = 0;
      var actualSws = er.ReadNew<StructArraySequenceWrapper>();
      Assert.AreEqual(expectedSw, actualSws);
    }

    [Test]
    public void TestWriteAndReadArrayValues() {
      var expectedSw = new StructArraySequenceWrapper {
          Values = new[] {
              new SchemaStruct { Value = 1 },
              new SchemaStruct { Value = 2 },
              new SchemaStruct { Value = 3 }
          }
      };

      var ms = new MemoryStream();

      var endianness = Endianness.BigEndian;

      var ew = new SchemaBinaryWriter(endianness);
      expectedSw.Write(ew);
      ew.CompleteAndCopyToDelayed(ms).Wait();

      var er = new SchemaBinaryReader(ms, endianness);
      er.Position = 1;
      var actualSws = er.ReadNewArray<SchemaStruct>(expectedSw.Values.Length);
      Assert.True(expectedSw.Values.SequenceEqual(actualSws));
    }


    [BinarySchema]
    public partial class StructListSequenceWrapper : IBinaryConvertible {
      [SequenceLengthSource(SchemaIntegerType.BYTE)]
      public List<SchemaStruct> Values { get; set; } = new();

      public override bool Equals(object other) {
        if (other is StructListSequenceWrapper otherSequenceWrapper) {
          return this.Values.SequenceEqual(otherSequenceWrapper.Values);
        }

        return false;
      }
    }

    [Test]
    public void TestWriteAndReadListObject() {
      var expectedSw = new StructListSequenceWrapper {
          Values = new List<SchemaStruct> {
              new() { Value = 1 }, new() { Value = 2 }, new() { Value = 3 }
          }
      };

      var ms = new MemoryStream();

      var endianness = Endianness.BigEndian;

      var ew = new SchemaBinaryWriter(endianness);
      expectedSw.Write(ew);
      ew.CompleteAndCopyToDelayed(ms).Wait();

      ms.Position = 0;
      var er = new SchemaBinaryReader(ms, endianness);
      var actualSw = er.ReadNew<StructListSequenceWrapper>();
      Assert.AreEqual(expectedSw, actualSw);
    }
  }
}