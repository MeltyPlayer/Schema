using System;

using schema.util.diagnostics;


namespace schema.binary.attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class NullTerminatedStringAttribute : BMemberAttribute<string> {
  protected override void InitFields(
      IDiagnosticReporter diagnosticReporter,
      IMemberReference memberThisIsAttachedTo) {
    if (!memberThisIsAttachedTo.IsString) {
      diagnosticReporter.ReportDiagnostic(
          memberThisIsAttachedTo.MemberSymbol,
          Rules.NullTerminatedStringCanOnlyBeUsedOnStrings);
    }
  }
}