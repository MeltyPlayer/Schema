using NUnit.Framework;


namespace schema.binary.text {
  internal class VarianceGeneratorTests {
    [Test]
    [TestCase("in")]
    [TestCase("out")]
    public void TestSupportsEachTypeOfVarianceInType(string variance) {
      BinarySchemaTestUtil.AssertGenerated(
          $$"""

            using schema.binary;

            namespace foo.bar {
              [BinarySchema]
              public partial class Wrapper<{{variance}} T>;
            }
            """,
          $$"""
          using System;
          using schema.binary;

          namespace foo.bar {
            public partial class Wrapper<{{variance}} T> {
              public void Read(IBinaryReader br) {
              }
            }
          }

          """,
          $$"""
          using System;
          using schema.binary;

          namespace foo.bar {
            public partial class Wrapper<{{variance}} T> {
              public void Write(IBinaryWriter bw) {
              }
            }
          }

          """);
    }
  }
}