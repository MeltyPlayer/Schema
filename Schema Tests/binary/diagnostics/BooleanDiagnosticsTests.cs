using NUnit.Framework;

using schema.binary.validators;


namespace schema.binary;

public class BooleanDiagnosticTests {
  [Test]
  public void TestBooleanWithoutAltFormat() {
    var structure = BinarySchemaTestUtil.ParseFirst("""

                                                    namespace foo.bar {
                                                      [BinarySchema]
                                                      public partial class BooleanWrapper {
                                                        public bool field;
                                                      }
                                                    }
                                                    """);
    BinarySchemaTestUtil.AssertDiagnostics(
        structure.Diagnostics,
        Rules.BooleanNeedsIntegerFormat,
        RequiredIntegerFormatAttributeValidator.BOOLEAN_NEEDS_INTEGER_FORMAT_RULE.DiagnosticDescriptor);
  }
}