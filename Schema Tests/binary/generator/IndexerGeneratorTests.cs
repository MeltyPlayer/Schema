using NUnit.Framework;


namespace schema.binary.text;

internal class IndexerGeneratorTests {
  [Test]
  public void TestIgnoresIndexers() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;

        [BinarySchema]
        public partial class ByteWrapper {
          public int this[int index] { get; set; }
        }
        """,
        """
        namespace foo.bar;

        /// <remarks>
        ///   Size: 0x0<br/>
        ///<br/>
        ///   Offsets: (none)<br/>
        /// </remarks>
        public partial class ByteWrapper {
          public const int SCHEMA_SIZE = 0x0;
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class ByteWrapper {
          public void Read(IBinaryReader br) {
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;
        
        public partial class ByteWrapper {
          public void Write(IBinaryWriter bw) {
          }
        }

        """);
  }
}