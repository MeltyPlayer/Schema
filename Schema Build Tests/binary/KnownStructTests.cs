using System.Collections.Generic;
using System.IO;
using System.Numerics;

using NUnit.Framework;

using schema.binary.attributes;
using schema.testing;
using schema.util.asserts;


namespace schema.binary;

public partial class KnownStructTests {
  [BinarySchema]
  private partial class Vector2Wrapper : IBinaryConvertible {
    public Vector2 Value { get; private set; }
  }

  [Test]
  public void TestReadingVector2() {
    using var br = SchemaMemoryStream.From([123f, 456f]).GetBinaryReader();
    Assert.AreEqual(new Vector2(123, 456), br.ReadNew<Vector2Wrapper>().Value);
  }

  [Test]
  public void TestWritingVector2() {
    var bw = new SchemaBinaryWriter();
    bw.WriteVector2(new Vector2(123, 456));

    using var ms = new MemoryStream();
    bw.CompleteAndCopyTo(ms);
    ms.Position = 0;
    Assert.AreEqual(2 * 4, ms.Length);

    var br = new SchemaBinaryReader(ms);
    Assert.AreEqual(123f, br.ReadSingle());
    Assert.AreEqual(456f, br.ReadSingle());
  }


  [BinarySchema]
  private partial class Vector2ListWrapper : IBinaryConvertible {
    [SequenceLengthSource(2)]
    public List<Vector2> Values { get; } = new();
  }

  [Test]
  public void TestReadingVector2List() {
    using var br = SchemaMemoryStream.From([123f, 234f, 345f, 456f])
                                     .GetBinaryReader();
    Asserts.Equal([new Vector2(123, 234), new Vector2(345, 456)],
                  br.ReadNew<Vector2ListWrapper>().Values);
  }

  [Test]
  public void TestWritingVector2List() {
    var bw = new SchemaBinaryWriter();
    bw.WriteVector2s([new Vector2(123, 234), new Vector2(345, 456)]);

    using var ms = new MemoryStream();
    bw.CompleteAndCopyTo(ms);
    ms.Position = 0;
    Assert.AreEqual(4 * 4, ms.Length);

    var br = new SchemaBinaryReader(ms);
    Assert.AreEqual(123f, br.ReadSingle());
    Assert.AreEqual(234f, br.ReadSingle());
    Assert.AreEqual(345f, br.ReadSingle());
    Assert.AreEqual(456f, br.ReadSingle());
  }

  [BinarySchema]
  private partial class Vector4Wrapper : IBinaryConvertible {
    public Vector4 Value { get; private set; }
  }

  [Test]
  public void TestReadingVector4() {
    using var br = SchemaMemoryStream.From([12f, 23f, 34f, 45f])
                                     .GetBinaryReader();
    Assert.AreEqual(new Vector4(12, 23, 34, 45),
                    br.ReadNew<Vector4Wrapper>().Value);
  }

  [Test]
  public void TestWritingVector4() {
    var bw = new SchemaBinaryWriter();
    bw.WriteVector4(new Vector4(12, 23, 34, 45));

    using var ms = new MemoryStream();
    bw.CompleteAndCopyTo(ms);
    ms.Position = 0;
    Assert.AreEqual(4 * 4, ms.Length);

    var br = new SchemaBinaryReader(ms);
    Assert.AreEqual(12f, br.ReadSingle());
    Assert.AreEqual(23f, br.ReadSingle());
    Assert.AreEqual(34f, br.ReadSingle());
    Assert.AreEqual(45f, br.ReadSingle());
  }


  [BinarySchema]
  private partial class QuaternionWrapper : IBinaryConvertible {
    public Quaternion Value { get; private set; }
  }

  [Test]
  public void TestReadingQuaternion() {
    using var br = SchemaMemoryStream.From([12f, 23f, 34f, 45f])
                                     .GetBinaryReader();
    Assert.AreEqual(new Quaternion(12, 23, 34, 45),
                    br.ReadNew<QuaternionWrapper>().Value);
  }

  [Test]
  public void TestWritingQuaternion() {
    var bw = new SchemaBinaryWriter();
    bw.WriteQuaternion(new Quaternion(12, 23, 34, 45));

    using var ms = new MemoryStream();
    bw.CompleteAndCopyTo(ms);
    ms.Position = 0;
    Assert.AreEqual(4 * 4, ms.Length);

    var br = new SchemaBinaryReader(ms);
    Assert.AreEqual(12f, br.ReadSingle());
    Assert.AreEqual(23f, br.ReadSingle());
    Assert.AreEqual(34f, br.ReadSingle());
    Assert.AreEqual(45f, br.ReadSingle());
  }
}