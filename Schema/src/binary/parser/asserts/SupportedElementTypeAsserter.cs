using System.Collections.Generic;

using Microsoft.CodeAnalysis;


namespace schema.binary.parser.asserts {
  internal class SupportedElementTypeAsserter {
    private readonly IList<Diagnostic> diagnostics_;

    public SupportedElementTypeAsserter(IList<Diagnostic> diagnostics) {
      this.diagnostics_ = diagnostics;
    }

    public void AssertElementTypesAreSupported(
        ISymbol memberSymbol,
        IMemberType memberType) {
      if (memberType is not ISequenceMemberType sequenceMemberType) {
        return;
      }

      var elementTypeInfo = sequenceMemberType.ElementType.TypeInfo;
      if (elementTypeInfo is IStructureTypeInfo structureTypeInfo) {
        if (!SymbolTypeUtil.Implements(structureTypeInfo.NamedTypeSymbol,
                                       typeof(IBinaryConvertible))) {
          this.diagnostics_.Add(
              Rules.CreateDiagnostic(
                  memberSymbol,
                  Rules.ElementNeedsToImplementIBiSerializable));
        }
      } else {
        if (elementTypeInfo.Kind == SchemaTypeKind.SEQUENCE) {
          this.diagnostics_.Add(
              Rules.CreateDiagnostic(
                  memberSymbol,
                  Rules.UnsupportedArrayType));
        }
      }
    }
  }
}