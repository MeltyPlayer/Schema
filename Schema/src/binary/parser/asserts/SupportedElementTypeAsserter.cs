using System.Collections.Generic;

using Microsoft.CodeAnalysis;

using schema.util.diagnostics;


namespace schema.binary.parser.asserts {
  internal class SupportedElementTypeAsserter {
    public void AssertElementTypesAreSupported(
        IDiagnosticReporter diagnosticReporter,
        IMemberType memberType) {
      if (memberType is not ISequenceMemberType sequenceMemberType) {
        return;
      }

      // Allow any type in sequence, assume it will be handled by the implementation.
      if (sequenceMemberType.SequenceTypeInfo.SequenceType is SequenceType
              .MUTABLE_SEQUENCE or SequenceType.READ_ONLY_SEQUENCE) {
        return;
      }

      var elementTypeInfo = sequenceMemberType.ElementType.TypeInfo;
      if (elementTypeInfo is IStructureTypeInfo structureTypeInfo) {
        if (!SymbolTypeUtil.Implements(structureTypeInfo.NamedTypeSymbol,
                                       typeof(IBinaryConvertible))) {
          diagnosticReporter.ReportDiagnostic(
              Rules.ElementNeedsToImplementIBiSerializable);
        }
      } else {
        if (elementTypeInfo.Kind == SchemaTypeKind.SEQUENCE) {
          diagnosticReporter.ReportDiagnostic(Rules.UnsupportedArrayType);
        }
      }
    }
  }
}