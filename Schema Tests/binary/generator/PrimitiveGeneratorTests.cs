using NUnit.Framework;

namespace schema.binary.text;

internal class PrimitiveGeneratorTests {
  [Test]
  [TestCase("byte", "Byte")]
  [TestCase("sbyte", "SByte")]
  [TestCase("short", "Int16")]
  public void TestBasicPrimitive(string primitiveType, string primitiveLabel) {
    BinarySchemaTestUtil.AssertGenerated(
        $$"""
          using schema.binary;

          namespace foo.bar;
           
          [BinarySchema]
          public partial class Wrapper {
            public {{primitiveType}} Field { get; set; }
            public {{primitiveType}} ReadonlyField { get; }
          }
          """,
        $$"""
          using System;
          using schema.binary;

          namespace foo.bar;

          public partial class Wrapper {
            public void Read(IBinaryReader br) {
              this.Field = br.Read{{primitiveLabel}}();
              br.Assert{{primitiveLabel}}(this.ReadonlyField);
            }
          }

          """,
        $$"""
          using System;
          using schema.binary;

          namespace foo.bar;

          public partial class Wrapper {
            public void Write(IBinaryWriter bw) {
              bw.Write{{primitiveLabel}}(this.Field);
              bw.Write{{primitiveLabel}}(this.ReadonlyField);
            }
          }

          """);
  }

  [Test]
  [TestCase("int", "Int24", false)]
  [TestCase("float", "Half", true)]
  public void TestSpecialPrimitive(string primitiveType,
                                   string primitiveLabel,
                                   bool expectCast) {
    var schemaNumberTypeText = $"SchemaNumberType.{primitiveLabel.ToUpper()}";
    var castText = expectCast ? $"({primitiveType}) " : "";

    BinarySchemaTestUtil.AssertGenerated(
        $$"""
          using schema.binary;
          using schema.binary.attributes;
          
          namespace foo.bar;
           
          [BinarySchema]
          public partial class Wrapper {
            [NumberFormat({{schemaNumberTypeText}})]
            public {{primitiveType}} Field { get; set; }

            [NumberFormat({{schemaNumberTypeText}})]
            public {{primitiveType}} ReadonlyField { get; }
            
            [NumberFormat({{schemaNumberTypeText}})]
            public readonly {{primitiveType}}[] ArrayField = new {{primitiveType}}[5];
          }
          """,
        $$"""
          using System;
          using schema.binary;

          namespace foo.bar;

          public partial class Wrapper {
            public void Read(IBinaryReader br) {
              this.Field = {{castText}}br.Read{{primitiveLabel}}();
              br.Assert{{primitiveLabel}}({{castText}}this.ReadonlyField);
              for (var i = 0; i < this.ArrayField.Length; ++i) {
                this.ArrayField[i] = {{castText}}br.Read{{primitiveLabel}}();
              }
            }
          }

          """,
        $$"""
          using System;
          using schema.binary;

          namespace foo.bar;

          public partial class Wrapper {
            public void Write(IBinaryWriter bw) {
              bw.Write{{primitiveLabel}}({{castText}}this.Field);
              bw.Write{{primitiveLabel}}({{castText}}this.ReadonlyField);
              for (var i = 0; i < this.ArrayField.Length; ++i) {
                bw.Write{{primitiveLabel}}({{castText}}this.ArrayField[i]);
              }
            }
          }

          """);
  }
}