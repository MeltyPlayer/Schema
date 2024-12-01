using System.Collections.Generic;
using System.IO;
using System.Numerics;

using NUnit.Framework;

using schema.binary.attributes;
using schema.testing;
using schema.util.asserts;


namespace schema.binary;

public partial class FixedPointTests {
  [BinarySchema]
  private partial class FixedPointWrapper : IBinaryConvertible {
    [FixedPoint(1, 19, 12)]
    public float FloatValue { get; set; }

    [FixedPoint(1, 19, 12)]
    public double DoubleValue { get; set; }
  }

  [Test]
  public void TestReadsAsExpected() {
    using var br = SchemaMemoryStream.From([123, 456]).GetBinaryReader();

    var wrapper = br.ReadNew<FixedPointWrapper>();
    Assert.AreEqual(0.0300292969f, wrapper.FloatValue);
    Assert.AreEqual(0.111328125, wrapper.DoubleValue);
  }

  [Test]
  public void TestWritesAsExpected() {
    var wrapper = new FixedPointWrapper {
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
}