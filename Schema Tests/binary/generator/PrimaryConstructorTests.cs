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
          namespace foo.bar;

          /// <remarks>
          ///   Size: 0x0<br/>
          ///<br/>
          ///   Offsets: (none)<br/>
          /// </remarks>
          public partial class Wrapper {
            public const int SCHEMA_SIZE = 0x0;
          }

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