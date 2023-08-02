using Microsoft.CodeAnalysis;

using System.Collections.Generic;

using schema.binary.parser;
using schema.util.diagnostics;


namespace schema.binary.attributes {
  internal class WPointerToParser : IAttributeParser {
    public void ParseIntoMemberType(IDiagnosticReporter diagnosticReporter,
                                    ISymbol memberSymbol,
                                    ITypeInfo memberTypeInfo,
                                    IMemberType memberType) {
      var pointerToAttribute = SymbolTypeUtil.GetAttribute<WPointerToAttribute>(
          diagnosticReporter,
          memberSymbol);
      if (pointerToAttribute == null) {
        return;
      }

      AccessChainUtil.AssertAllNodesInTypeChainUntilTargetUseBinarySchema(
          diagnosticReporter,
          pointerToAttribute.AccessChainToOtherMember);

      if (memberTypeInfo is IIntegerTypeInfo &&
          memberType is BinarySchemaStructureParser.PrimitiveMemberType
              primitiveMemberType) {
        primitiveMemberType.AccessChainToPointer =
            pointerToAttribute.AccessChainToOtherMember;
      } else {
        diagnosticReporter.ReportDiagnostic(Rules.NotSupported);
      }
    }
  }
}