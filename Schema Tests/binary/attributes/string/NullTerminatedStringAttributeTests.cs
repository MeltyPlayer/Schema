using NUnit.Framework;


namespace schema.binary.attributes;

internal class NullTerminatedStringAttributeTests {
  [Test]
  public void TestNullTerminatedString() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;

        [BinarySchema]
        public partial class intsWrapper : IBinaryConvertible {
          [NullTerminatedString]
          public string Field { get; set; }
        }
        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class intsWrapper {
          public void Read(IBinaryReader br) {
            this.Field = br.ReadStringNT();
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class intsWrapper {
          public void Write(IBinaryWriter bw) {
            bw.WriteStringNT(this.Field);
          }
        }

        """);
  }
}