using NUnit.Framework;


namespace schema.binary.attributes;

internal class ReadLogicAttributeTests {
  [Test]
  public void TestAttribute() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;
        
        [BinarySchema]
        public partial class Wrapper : IBinaryConvertible {
          public byte Field1 { get; set; }
      
          [ReadLogic]
          public void Method(IBinaryReader br) {}
      
          public byte Field2 { get; set; }
        }
        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;
        
        public partial class Wrapper {
          public void Read(IBinaryReader br) {
            this.Field1 = br.ReadByte();
            this.Method(br);
            this.Field2 = br.ReadByte();
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;
        
        public partial class Wrapper {
          public void Write(IBinaryWriter bw) {
            bw.WriteByte(this.Field1);
            bw.WriteByte(this.Field2);
          }
        }

        """);
  }
}