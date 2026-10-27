using NUnit.Framework;

namespace schema.binary.text;

internal class PrimitiveGeneratorTests {
  [Test]
  [TestCase("byte", "Byte", 1)]
  [TestCase("sbyte", "SByte", 1)]
  [TestCase("short", "Int16", 2)]
  public void TestBasicPrimitive(string primitiveType, string primitiveLabel, int size) {
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
          namespace foo.bar;

          /// <remarks>
          ///   Size: 0x{{2*size:X}}<br/>
          ///<br/>
          ///   Offsets:<br/>
          ///     - Field: 0x0<br/>
          ///     - ReadonlyField: 0x{{size:X}}<br/>
          /// </remarks>
          public partial class Wrapper {
            public const int SCHEMA_SIZE = 0x{{2*size:X}};
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
  [TestCase("int", "Int24", 3, false)]
  [TestCase("float", "Half", 2, true)]
  public void TestSpecialPrimitive(string primitiveType,
                                   string primitiveLabel,
                                   int size,
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
          $"""
          namespace foo.bar;

          /// <remarks>
          ///   Size: (indeterminate)<br/>
          ///<br/>
          ///   Offsets:<br/>
          ///     - Field: 0x0<br/>
          ///     - ReadonlyField: 0x{size:X}<br/>
          ///     - ArrayField: 0x{2*size:X}<br/>
          /// </remarks>
          public partial class Wrapper;

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