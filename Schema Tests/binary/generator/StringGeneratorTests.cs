using NUnit.Framework;

using schema.binary.attributes;


namespace schema.binary.text;

internal class StringGeneratorTests {
  [Test]
  [TestCase(StringEncodingType.UTF8)]
  [TestCase(StringEncodingType.UTF16)]
  [TestCase(StringEncodingType.UTF32)]
  [TestCase(StringEncodingType.SHIFT_JIS)]
  public void TestStringEncoding(StringEncodingType encodingType) {
    BinarySchemaTestUtil.AssertGenerated(
        $$"""

          using schema.binary;
          using schema.binary.attributes;

          namespace foo.bar;

          [BinarySchema]
          public partial class Wrapper {
            [StringLengthSource(10)]
            [StringEncoding(StringEncodingType.{{encodingType}})]
            public string Field { get; set; }
          }
          """,
        $$"""
          using System;
          using schema.binary;
          using schema.binary.attributes;

          namespace foo.bar;

          public partial class Wrapper {
            public void Read(IBinaryReader br) {
              this.Field = br.ReadString(StringEncodingType.{{encodingType}}, 10);
            }
          }

          """,
        $$"""
          using System;
          using schema.binary;
          using schema.binary.attributes;
          
          namespace foo.bar;

          public partial class Wrapper {
            public void Write(IBinaryWriter bw) {
              bw.WriteStringWithExactLength(StringEncodingType.{{encodingType}}, this.Field, 10);
            }
          }

          """);
  }
}