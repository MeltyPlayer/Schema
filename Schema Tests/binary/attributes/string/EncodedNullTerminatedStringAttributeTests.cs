using NUnit.Framework;


namespace schema.binary.attributes;

internal class EncodedNullTerminatedStringAttributeTests {
  [Test]
  public void TestNullTerminatedString() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;
        
        [BinarySchema]
        public partial class intsWrapper : IBinaryConvertible {
          [StringEncoding(StringEncodingType.UTF8)]
          [NullTerminatedString]
          public string Field { get; set; }
        }
        """,
        """
        using System;
        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;
        
        public partial class intsWrapper {
          public void Read(IBinaryReader br) {
            this.Field = br.ReadStringNT(StringEncodingType.UTF8);
          }
        }

        """,
        """
        using System;
        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;
        
        public partial class intsWrapper {
          public void Write(IBinaryWriter bw) {
            bw.WriteStringNT(StringEncodingType.UTF8, this.Field);
          }
        }

        """);
  }
}