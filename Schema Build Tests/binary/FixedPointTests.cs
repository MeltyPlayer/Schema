using System.IO;

using NUnit.Framework;

using schema.binary.attributes;
using schema.testing;


namespace schema.binary;

public partial class FixedPointTests {
  [BinarySchema]
  private partial class UintFixedPointWrapper : IBinaryConvertible {
    [FixedPoint(1, 19, 12)]
    public float FloatValue { get; set; }

    [FixedPoint(1, 19, 12)]
    public double DoubleValue { get; set; }
  }

  [Test]
  public void TestReadsUintAsExpected() {
    using var br = SchemaMemoryStream.From([123, 456]).GetBinaryReader();

    var wrapper = br.ReadNew<UintFixedPointWrapper>();
    Assert.AreEqual(0.0300292969f, wrapper.FloatValue);
    Assert.AreEqual(0.111328125, wrapper.DoubleValue);
  }

  [Test]
  public void TestWritesUintAsExpected() {
    var wrapper = new UintFixedPointWrapper {
        FloatValue = 0.0300292969f, DoubleValue = 0.111328125,
    };

    var bw = new SchemaBinaryWriter();
    wrapper.Write(bw);

    using var ms = new MemoryStream();
    bw.CompleteAndCopyTo(ms);
    ms.Position = 0;

    var br = new SchemaBinaryReader(ms);
    Assert.AreEqual(123, br.ReadUInt32());
    Assert.AreEqual(456, br.ReadUInt32());
  }


  [BinarySchema]
  private partial class UshortFixedPointWrapper : IBinaryConvertible {
    [FixedPoint(1, 7, 8)]
    public float FloatValue { get; set; }

    [FixedPoint(1, 7, 8)]
    public double DoubleValue { get; set; }
  }

  [Test]
  public void TestReadsUshortAsExpected() {
    using var br = SchemaMemoryStream.From([(ushort) 123, (ushort) 456]).GetBinaryReader();

    var wrapper = br.ReadNew<UshortFixedPointWrapper>();
    Assert.AreEqual(0.48046875f, wrapper.FloatValue);
    Assert.AreEqual(1.78125d, wrapper.DoubleValue);
  }

  [Test]
  public void TestWritesUshortAsExpected() {
    var wrapper = new UshortFixedPointWrapper {
        FloatValue = 0.48046875f,
        DoubleValue = 1.78125d,
    };

    var bw = new SchemaBinaryWriter();
    wrapper.Write(bw);

    using var ms = new MemoryStream();
    bw.CompleteAndCopyTo(ms);
    ms.Position = 0;

    var br = new SchemaBinaryReader(ms);
    Assert.AreEqual(123, br.ReadUInt16());
    Assert.AreEqual(456, br.ReadUInt16());
  }
}