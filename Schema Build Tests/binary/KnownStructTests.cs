using System.Numerics;

using NUnit.Framework;

using schema.testing;


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


  [BinarySchema]
  private partial class Vector4Wrapper : IBinaryConvertible {
    public Vector4 Value { get; private set; }
  }

  [Test]
  public void TestReadingVector4() {
    using var br = SchemaMemoryStream.From([12f, 23f, 34f, 45f])
                                     .GetBinaryReader();
    Assert.AreEqual(new Vector4(12, 23, 34, 45), br.ReadNew<Vector4Wrapper>().Value);
  }
}