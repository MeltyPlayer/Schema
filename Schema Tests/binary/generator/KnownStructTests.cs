using NUnit.Framework;


namespace schema.binary.text;

internal class KnownStructTests {
  [Test]
  [TestCase("Vector2")]
  [TestCase("Vector3")]
  [TestCase("Vector4")]
  [TestCase("Matrix3x2")]
  [TestCase("Matrix4x4")]
  [TestCase("Quaternion")]
  public void TestSystemNumerics(string knownStructName) {
    BinarySchemaTestUtil.AssertGenerated(
        $$"""
          using System.Numerics;
          using schema.binary;

          namespace foo.bar;

          [BinarySchema]
          public partial class Wrapper {
            public {{knownStructName}} Field { get; set; }
            public {{knownStructName}} PrivateField { get; private set; }
          }
          """,
        $$"""
          using System;
          using schema.binary;

          namespace foo.bar;

          public partial class Wrapper {
            public void Read(IBinaryReader br) {
              this.Field = br.Read{{knownStructName}}();
              this.PrivateField = br.Read{{knownStructName}}();
            }
          }

          """,
        $$"""
          using System;
          using schema.binary;

          namespace foo.bar;

          public partial class Wrapper {
            public void Write(IBinaryWriter bw) {
              bw.Write{{knownStructName}}(this.Field);
              bw.Write{{knownStructName}}(this.PrivateField);
            }
          }

          """);
  }
}