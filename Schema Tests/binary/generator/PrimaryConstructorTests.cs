using NUnit.Framework;

namespace schema.binary.text;

internal class PrimaryConstructorTests {
  [Test]
  public void TestIgnoresPrimaryConstructorFields() {
    BinarySchemaTestUtil.AssertGenerated(
        """
          using schema.binary;
          using schema.binary.attributes;
          
          namespace foo.bar;

          [BinarySchema]
          public partial class Wrapper(int key) : IBinaryConvertible;
          """,
        """
          using System;
          using schema.binary;

          namespace foo.bar;

          public partial class Wrapper {
            public void Read(IBinaryReader br) {
            }
          }

          """,
        """
          using System;
          using schema.binary;

          namespace foo.bar;

          public partial class Wrapper {
            public void Write(IBinaryWriter bw) {
            }
          }

          """);
  }
}