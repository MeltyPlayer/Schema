using NUnit.Framework;


namespace schema.binary.text;

internal class BooleanGeneratorTests {
  [Test]
  [TestCase("byte", "Byte", true)]
  [TestCase("short", "Int16", true)]
  [TestCase("int", "Int32", false)]
  public void TestEachType(string primitiveType,
                           string primitiveLabel,
                           bool expectCast) {
    var schemaIntegerType = $"SchemaIntegerType.{primitiveLabel.ToUpper()}";
    var castText = expectCast ? $"({primitiveType}) " : "";

    BinarySchemaTestUtil.AssertGenerated(
        $$"""

          using schema.binary;
          using schema.binary.attributes;

          namespace foo.bar;

          [BinarySchema]
          public partial class Wrapper {
            [IntegerFormat({{schemaIntegerType}})]
            public bool Field { get; set; }
          
            [IntegerFormat({{schemaIntegerType}})]
            public bool ReadonlyField { get; }
          }
          """,
        $$"""
          using System;
          using schema.binary;

          namespace foo.bar;

          public partial class Wrapper {
            public void Read(IBinaryReader br) {
              this.Field = br.Read{{primitiveLabel}}() != 0;
              br.Assert{{primitiveLabel}}({{castText}}{{(expectCast ? "(" : "")}}this.ReadonlyField ? 1 : 0{{(expectCast ? ")" : "")}});
            }
          }

          """,
        $$"""
          using System;
          using schema.binary;

          namespace foo.bar;

          public partial class Wrapper {
            public void Write(IBinaryWriter bw) {
              bw.Write{{primitiveLabel}}({{castText}}{{(expectCast ? "(" : "")}}this.Field ? 1 : 0{{(expectCast ? ")" : "")}});
              bw.Write{{primitiveLabel}}({{castText}}{{(expectCast ? "(" : "")}}this.ReadonlyField ? 1 : 0{{(expectCast ? ")" : "")}});
            }
          }

          """);
  }


  [Test]
  public void TestByteArray() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;

        [BinarySchema]
        public partial class ByteWrapper {
          [SequenceLengthSource(4)]
          [IntegerFormat(SchemaIntegerType.BYTE)]
          public bool[] Field { get; set; }
        }
        """,
        """
        using System;
        using schema.binary;
        using schema.util.sequences;

        namespace foo.bar;

        public partial class ByteWrapper {
          public void Read(IBinaryReader br) {
            this.Field = SequencesUtil.CloneAndResizeSequence(this.Field, 4);
            for (var i = 0; i < this.Field.Length; ++i) {
              this.Field[i] = br.ReadByte() != 0;
            }
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class ByteWrapper {
          public void Write(IBinaryWriter bw) {
            for (var i = 0; i < this.Field.Length; ++i) {
              bw.WriteByte((byte) (this.Field[i] ? 1 : 0));
            }
          }
        }

        """);
  }
}