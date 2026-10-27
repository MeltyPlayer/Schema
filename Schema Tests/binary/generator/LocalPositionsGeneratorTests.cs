using NUnit.Framework;


namespace schema.binary.text;

internal class LocalPositionsGeneratorTests {
  [Test]
  public void TestLocalSpace() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;
        
        [BinarySchema]
        [LocalPositions]
        public partial class LocalPositionsWrapper : IBinaryConvertible {
          public byte Value { get; set; }
        }
        """,
        """
        namespace foo.bar;

        /// <remarks>
        ///   Size: 0x0<br/>
        ///<br/>
        ///   Offsets: (none)<br/>
        /// </remarks>
        public partial class LocalPositionsWrapper {
          public const int SCHEMA_SIZE = 0x0;
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class LocalPositionsWrapper {
          public void Read(IBinaryReader br) {
            br.PushLocalSpace();
            this.Value = br.ReadByte();
            br.PopLocalSpace();
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class LocalPositionsWrapper {
          public void Write(IBinaryWriter bw) {
            bw.PushLocalSpace();
            bw.WriteByte(this.Value);
            bw.PopLocalSpace();
          }
        }

        """);
  }
}