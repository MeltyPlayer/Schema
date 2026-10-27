using NUnit.Framework;


namespace schema.binary.text;

internal class BooleanGeneratorTests {
  [Test]
  [TestCase("byte", "Byte", 1, true)]
  [TestCase("short", "Int16", 2, true)]
  [TestCase("int", "Int32", 4, false)]
  public void TestEachType(string primitiveType,
                           string primitiveLabel,
                           int bytesPerBool,
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
          namespace foo.bar;

          /// <remarks>
          ///   Size: 0x{{2 * bytesPerBool}}<br/>
          ///<br/>
          ///   Offsets:<br/>
          ///     - Field: 0x0<br/>
          ///     - ReadonlyField: 0x{{bytesPerBool}}<br/>
          /// </remarks>
          public partial class Wrapper {
            public const int SCHEMA_SIZE = 0x{{2*bytesPerBool:X}};
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
        namespace foo.bar;

        /// <remarks>
        ///   Size: 0x4<br/>
        ///<br/>
        ///   Offsets:<br/>
        ///     - Field: 0x0<br/>
        /// </remarks>
        public partial class ByteWrapper {
          public const int SCHEMA_SIZE = 0x4;
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