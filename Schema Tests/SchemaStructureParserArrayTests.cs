using NUnit.Framework;

using schema.binary.validators;


namespace schema.binary;

public partial class SchemaStructureParserTests {
  public class Array {
    [Test]
    public void TestMutableArrayWithoutLength() {
      var structure = BinarySchemaTestUtil.ParseFirst("""

          namespace foo.bar {
            [BinarySchema]
            public partial class ArrayWrapper {
              public int[] field;
            }
          }
          """);
      BinarySchemaTestUtil.AssertDiagnostics(
          structure.Diagnostics,
          Rules.MutableArrayNeedsLengthSource,
          RequiredSequenceAttributeValidator.SEQUENCE_REQUIRES_ATTRIBUTE_RULE.DiagnosticDescriptor);
    }
  }
}