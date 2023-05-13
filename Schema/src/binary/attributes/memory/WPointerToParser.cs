using Microsoft.CodeAnalysis;

using System.Collections.Generic;

using schema.binary.parser;


namespace schema.binary.attributes.memory {
  internal class WPointerToParser {
    public void Parse(IList<Diagnostic> diagnostics,
                      ISymbol memberSymbol,
                      ITypeInfo memberTypeInfo,
                      IMemberType memberType) {
      var pointerToAttribute = SymbolTypeUtil.GetAttribute<WPointerToAttribute>(
          diagnostics, memberSymbol);
      if (pointerToAttribute == null) {
        return;
      }

      AccessChainUtil.AssertAllNodesInTypeChainUntilTargetUseBinarySchema(
          diagnostics, pointerToAttribute.AccessChainToOtherMember);

      if (memberTypeInfo is IIntegerTypeInfo &&
          memberType is BinarySchemaStructureParser.PrimitiveMemberType
              primitiveMemberType) {
        primitiveMemberType.AccessChainToPointer =
            pointerToAttribute.AccessChainToOtherMember;
      } else {
        diagnostics.Add(
            Rules.CreateDiagnostic(memberSymbol, Rules.NotSupported));
      }
    }
  }
}