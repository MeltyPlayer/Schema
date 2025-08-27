using System;

using schema.util.diagnostics;


namespace schema.binary.attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class FixedPointAttribute(
    int signBits,
    int integerBits,
    int fractionBits) : BMemberAttribute {
  public int SignBits { get; set; } = signBits;
  public int IntegerBits { get; set; } = integerBits;
  public int FractionBits { get; set; } = fractionBits;

  protected override void InitFields(IDiagnosticReporter diagnosticReporter,
                                     IMemberReference memberThisIsAttachedTo) {
    if (!memberThisIsAttachedTo.IsFloat) {
      diagnosticReporter.ReportDiagnostic(Rules.FixedPointCanOnlyBeUsedOnFloats);
    }
  }

  public SchemaIntegerType IntegerType
    => (this.SignBits + this.IntegerBits + this.FractionBits) switch {
        <= 8  => SchemaIntegerType.BYTE,
        <= 16 => SchemaIntegerType.UINT16,
        <= 24 => SchemaIntegerType.UINT24,
        <= 32 => SchemaIntegerType.UINT32,
        <= 64 => SchemaIntegerType.UINT64,
    };
}