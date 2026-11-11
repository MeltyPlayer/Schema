using NUnit.Framework;

using schema.binary.attributes;
using schema.binary.validators;


namespace schema.binary;

public class WPointerToDiagnosticsTests {
  [Test]
  [TestCase(false)]
  [TestCase(true)]
  public void TestNotAnInteger(bool isNullableField) {
    var structure = BinarySchemaTestUtil.ParseFirst(
        $$"""
          using schema.binary;
          using schema.binary.attributes;

          namespace foo.bar {
            [BinarySchema]
            public partial class Wrapper {
              [{{(isNullableField ? nameof(WPointerToOrNullAttribute) : nameof(WPointerToAttribute))}}(nameof(Wrapper.field))]
              public char pointer;
            
              public byte{{(isNullableField ? "?" : "")}} field;
            }
          }
          """);
    BinarySchemaTestUtil.AssertDiagnostics(
        structure.Diagnostics,
        isNullableField
            ? WPointerToCanOnlyBeUsedOnIntegersValidator
              .WPOINTER_TO_OR_NULL_CAN_ONLY_BE_USED_ON_INTEGERS_RULE
              .DiagnosticDescriptor
            : WPointerToCanOnlyBeUsedOnIntegersValidator
              .WPOINTER_TO_CAN_ONLY_BE_USED_ON_INTEGERS_RULE
              .DiagnosticDescriptor);
  }
  
  [Test]
  [TestCase(false)]
  [TestCase(true)]
  public void TestWrongNullability(bool isNullableField) {
    var structure = BinarySchemaTestUtil.ParseFirst(
        $$"""
          using schema.binary;
          using schema.binary.attributes;
          
          namespace foo.bar {
            [BinarySchema]
            public partial class Wrapper {
              [{{(isNullableField ? nameof(WPointerToAttribute) : nameof(WPointerToOrNullAttribute))}}(nameof(Wrapper.field))]
              public int pointer;
            
              public byte{{(isNullableField ? "?" : "")}} field;
            }
          }
          """);
    BinarySchemaTestUtil.AssertDiagnostics(
        structure.Diagnostics,
        isNullableField
            ? WPointerToNeedsToBeUsedOnRightNullabilityValidator
              .WPOINTER_TO_CAN_ONLY_BE_USED_ON_NON_NULLABLE_RULE
              .DiagnosticDescriptor
            : WPointerToNeedsToBeUsedOnRightNullabilityValidator
              .WPOINTER_TO_OR_NULL_CAN_ONLY_BE_USED_ON_NULLABLE_RULE
              .DiagnosticDescriptor);
  }
}