using System.Collections.Generic;
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



    [BinarySchema]
    public partial class Uint32LengthSequenceWrapper : IBinaryConvertible {
      [SequenceLengthSource(SchemaIntegerType.UINT32)]
      public int[] Values { get; set; }

      public override bool Equals(object other) {
        if (other is Uint32LengthSequenceWrapper otherSequenceWrapper) {
          return this.Values.SequenceEqual(otherSequenceWrapper.Values);
        }

        return false;
      }
    }

    [Test]
    public void TestWriteAndReadWithUint32Length() {
      var expectedSw = new Uint32LengthSequenceWrapper {
          Values = new[] { 1, 2, 3, 4, 5, 9, 8, 7, 6 }
      };

      var ms = new MemoryStream();

      var endianness = Endianness.BigEndian;
      var ew = new EndianBinaryWriter(endianness);

      expectedSw.Write(ew);
      ew.CompleteAndCopyToDelayed(ms).Wait();

      ms.Position = 0;
      var er = new EndianBinaryReader(ms, endianness);
      var actualSw = er.ReadNew<Uint32LengthSequenceWrapper>();

      Assert.AreEqual(expectedSw, actualSw);
    }



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
    public void TestWriteAndReadNewArray() {
      var expectedSw = new StructArraySequenceWrapper {
        Values = new[]
        {
          new SchemaStruct { Value = 1 },
          new SchemaStruct { Value = 2 },
          new SchemaStruct { Value = 3 }
        }
      };

      var ms = new MemoryStream();

      var endianness = Endianness.BigEndian;
      var ew = new EndianBinaryWriter(endianness);

      expectedSw.Write(ew);
      ew.CompleteAndCopyToDelayed(ms).Wait();

      var er = new EndianBinaryReader(ms, endianness);

      er.Position = 1;
      var actualSws = er.ReadNewArray<SchemaStruct>(expectedSw.Values.Length);
      Assert.True(expectedSw.Values.SequenceEqual(actualSws));
    }


    [Test]
    public void TestWriteAndReadStructArray() {
      var expectedSw = new StructArraySequenceWrapper {
        Values = new[]
        {
          new SchemaStruct { Value = 1 },
          new SchemaStruct { Value = 2 },
          new SchemaStruct { Value = 3 }
        }
      };

      var ms = new MemoryStream();

      var endianness = Endianness.BigEndian;
      var ew = new EndianBinaryWriter(endianness);

      expectedSw.Write(ew);
      ew.CompleteAndCopyToDelayed(ms).Wait();

      ms.Position = 0;
      var er = new EndianBinaryReader(ms, endianness);
      var actualSw = er.ReadNew<StructArraySequenceWrapper>();

      Assert.AreEqual(expectedSw, actualSw);

      er.Position = 1;
      var actualSws = er.ReadNewArray<SchemaStruct>(expectedSw.Values.Length);
      Assert.True(expectedSw.Values.SequenceEqual(actualSws));
    }


    
    [BinarySchema]
    public partial class StructListSequenceWrapper : IBinaryConvertible
    {
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
    public void TestWriteAndReadStructs() {
      var expectedSw = new StructListSequenceWrapper {
        Values = new List<SchemaStruct>
        {
          new() { Value = 1 },
          new() { Value = 2 },
          new() { Value = 3 }
        }
      };

      var ms = new MemoryStream();

      var endianness = Endianness.BigEndian;
      var ew = new EndianBinaryWriter(endianness);

      expectedSw.Write(ew);
      ew.CompleteAndCopyToDelayed(ms).Wait();

      ms.Position = 0;
      var er = new EndianBinaryReader(ms, endianness);
      var actualSw = er.ReadNew<StructListSequenceWrapper>();

      Assert.AreEqual(expectedSw, actualSw);
    }
  }
}