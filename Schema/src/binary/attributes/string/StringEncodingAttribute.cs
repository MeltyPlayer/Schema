using System;

using schema.util.diagnostics;


namespace schema.binary.attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class StringEncodingAttribute : BMemberAttribute<string> {
  public StringEncodingAttribute(StringEncodingType encodingType) {
    this.EncodingType = encodingType;
  }

  protected override void InitFields(
      IDiagnosticReporter diagnosticReporter,
      IMemberReference memberThisIsAttachedTo) {
    if (!memberThisIsAttachedTo.IsString) {
      diagnosticReporter.ReportDiagnostic(
          memberThisIsAttachedTo.MemberSymbol,
          Rules.StringEncodingCanOnlyBeUsedOnStrings);
    }
  }

  public StringEncodingType EncodingType { get; private set; }
}