using schema.util.diagnostics;
using schema.util.symbols;


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
      if (elementTypeInfo is IContainerTypeInfo containerTypeInfo) {
        if (!containerTypeInfo.TypeV2.Implements<IBinaryConvertible>()) {
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