﻿using Microsoft.CodeAnalysis;

using schema.util.diagnostics;


namespace schema.binary.parser.asserts;

internal class SupportedElementTypeAsserter {
  public void AssertElementTypesAreSupported(
      IDiagnosticReporter diagnosticReporter,
      ITypeSymbol containerTypeV2,
      IMemberType memberType) {
    if (memberType is not ISequenceMemberType sequenceMemberType) {
      return;
    }

    // Allow any type in sequence, assume it will be handled by the implementation.
    if (sequenceMemberType.SequenceTypeInfo.SequenceType is SequenceType
            .MUTABLE_SEQUENCE
        or SequenceType.READ_ONLY_SEQUENCE) {
      return;
    }

    var elementTypeInfo = sequenceMemberType.ElementType.TypeInfo;
    if (elementTypeInfo is IContainerTypeInfo elementContainerTypeInfo) {
      var elementContainerTypeV2 = elementContainerTypeInfo.TypeSymbol;
      if (!elementContainerTypeV2.IsAtLeastAsBinaryConvertibleAs(
              containerTypeV2)) {
        diagnosticReporter.ReportDiagnostic(
            Rules.ElementBinaryConvertabilityNeedsToSatisfyParent);
      }
    } else {
      if (elementTypeInfo.Kind == SchemaTypeKind.SEQUENCE) {
        diagnosticReporter.ReportDiagnostic(Rules.UnsupportedArrayType);
      }
    }
  }
}