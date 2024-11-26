using System;
using System.Linq;

using NUnit.Framework;


namespace schema.binary.text;

internal class KnownStructTests {
  private static readonly (string, int)[] KNOWN_STRUCT_AND_LENGTH_TEST_CASES = [
      ("Vector2", 2 * 4),
      ("Vector3", 3 * 4),
      ("Vector4", 4 * 4),
      ("Matrix3x2", 3 * 2 * 4),
      ("Matrix4x4", 4 * 4 * 4),
      ("Quaternion", 4 * 4)
  ];

  private static readonly string[] KNOWN_STRUCT_TEST_CASES
      = KNOWN_STRUCT_AND_LENGTH_TEST_CASES.Select(e => e.Item1).ToArray();

  [Test]
  [TestCaseSource(nameof(KNOWN_STRUCT_TEST_CASES))]
  public void TestSystemNumericsField(string knownStructName)
    => BinarySchemaTestUtil.AssertGenerated(
        $$"""
          using System.Numerics;
          using schema.binary;

          namespace foo.bar;

          [BinarySchema]
          public partial class Wrapper {
            public {{knownStructName}} Field { get; set; }
            public {{knownStructName}} PrivateField { get; private set; }
          }
          """,
        $$"""
          using System;
          using schema.binary;

          namespace foo.bar;

          public partial class Wrapper {
            public void Read(IBinaryReader br) {
              this.Field = br.Read{{knownStructName}}();
              this.PrivateField = br.Read{{knownStructName}}();
            }
          }

          """,
        $$"""
          using System;
          using schema.binary;

          namespace foo.bar;

          public partial class Wrapper {
            public void Write(IBinaryWriter bw) {
              bw.Write{{knownStructName}}(this.Field);
              bw.Write{{knownStructName}}(this.PrivateField);
            }
          }

          """);

  [Test]
  [TestCaseSource(nameof(KNOWN_STRUCT_TEST_CASES))]
  public void TestSystemNumericsReadonlyArray(string knownStructName)
    => BinarySchemaTestUtil.AssertGenerated(
        $$"""
          using System.Numerics;
          using schema.binary;

          namespace foo.bar;

          [BinarySchema]
          public partial class Wrapper {
            public {{knownStructName}}[] Field { get; }
          }
          """,
        $$"""
          using System;
          using schema.binary;

          namespace foo.bar;

          public partial class Wrapper {
            public void Read(IBinaryReader br) {
              br.Read{{knownStructName}}s(this.Field);
            }
          }

          """,
        $$"""
          using System;
          using schema.binary;

          namespace foo.bar;

          public partial class Wrapper {
            public void Write(IBinaryWriter bw) {
              bw.Write{{knownStructName}}s(this.Field);
            }
          }

          """);

  [Test]
  [TestCaseSource(nameof(KNOWN_STRUCT_AND_LENGTH_TEST_CASES))]
  public void TestSystemNumericsUntilEndOfStreamArray(
      (string, int) knownStructNameAndLength) {
    var (knownStructName, knownStructLength) = knownStructNameAndLength;

    var sizeLog2 = Math.Log2(knownStructLength);
    var sizeDivisionText = (sizeLog2 % 1 == 0)
        ? $" >> {sizeLog2}"
        : $" / {knownStructLength}";

    BinarySchemaTestUtil.AssertGenerated(
        $$"""
          using System.Numerics;
          using schema.binary;
          using schema.binary.attributes;

          namespace foo.bar;

          [BinarySchema]
          public partial class Wrapper {
            [RSequenceUntilEndOfStream]
            public {{knownStructName}}[] Field { get; set; }
          }
          """,
        $$"""
          using System;
          using System.Collections.Generic;
          using schema.binary;

          namespace foo.bar;

          public partial class Wrapper {
            public void Read(IBinaryReader br) {
              this.Field = br.Read{{knownStructName}}s((br.Length - br.Position){{sizeDivisionText}});
            }
          }

          """,
        $$"""
          using System;
          using schema.binary;

          namespace foo.bar;

          public partial class Wrapper {
            public void Write(IBinaryWriter bw) {
              bw.Write{{knownStructName}}s(this.Field);
            }
          }

          """);
  }

  [Test]
  [TestCaseSource(nameof(KNOWN_STRUCT_TEST_CASES))]
  public void TestSystemNumericsList(string knownStructName)
    => BinarySchemaTestUtil.AssertGenerated(
        $$"""
          using System.Collections.Generic;
          using System.Numerics;
          using schema.binary;
          using schema.binary.attributes;

          namespace foo.bar;

          [BinarySchema]
          public partial class Wrapper {
            [SequenceLengthSource(10)]
            public List<{{knownStructName}}> Field { get; set; }
          }
          """,
        $$"""
          using System;
          using schema.binary;
          using schema.util.sequences;

          namespace foo.bar;

          public partial class Wrapper {
            public void Read(IBinaryReader br) {
              SequencesUtil.ResizeSequenceInPlace(this.Field, 10);
              br.Read{{knownStructName}}s(this.Field.AsSpan());
            }
          }

          """,
        $$"""
          using System;
          using schema.binary;

          namespace foo.bar;

          public partial class Wrapper {
            public void Write(IBinaryWriter bw) {
              bw.Write{{knownStructName}}s(this.Field.AsSpan());
            }
          }

          """);
}