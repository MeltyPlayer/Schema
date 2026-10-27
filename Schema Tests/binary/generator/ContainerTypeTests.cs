using NUnit.Framework;


namespace schema.binary.text;

internal class ContainerTypeTests {
  [Test]
  [TestCase("struct")]
  [TestCase("class")]
  [TestCase("record")]
  [TestCase("record struct")]
  public void TestEachType(string containerType) {
    BinarySchemaTestUtil.AssertGenerated(
        $$"""

          using schema.binary;
          using schema.binary.attributes;

          namespace foo.bar;

          [BinarySchema]
          public partial {{containerType}} Wrapper {
            public int Field { get; set; }
          }
          """,
        $$"""
          namespace foo.bar;

          /// <remarks>
          ///   Size: 0x4<br/>
          ///<br/>
          ///   Offsets:<br/>
          ///     - Field: 0x0<br/>
          /// </remarks>
          public partial {{containerType}} Wrapper {
            public const int SCHEMA_SIZE = 0x4;
          }

          """,
        $$"""
          using System;
          using schema.binary;

          namespace foo.bar;

          public partial {{containerType}} Wrapper {
            public void Read(IBinaryReader br) {
              this.Field = br.ReadInt32();
            }
          }

          """,
        $$"""
          using System;
          using schema.binary;

          namespace foo.bar;

          public partial {{containerType}} Wrapper {
            public void Write(IBinaryWriter bw) {
              bw.WriteInt32(this.Field);
            }
          }

          """);
  }
}