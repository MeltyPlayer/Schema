using Microsoft.CodeAnalysis;

using System.Collections.Generic;

using schema.binary.parser;


namespace schema.binary.attributes.size {
  internal class WSizeOfMemberInBytesParser {
    public void Parse(IList<Diagnostic> diagnostics,
                      ISymbol memberSymbol,
                      ITypeInfo memberTypeInfo,
                      IMemberType memberType) {
      var sizeOfAttribute =
          SymbolTypeUtil.GetAttribute<WSizeOfMemberInBytesAttribute>(
              diagnostics, memberSymbol);
      if (sizeOfAttribute == null) {
        return;
      }

      AccessChainUtil.AssertAllNodesInTypeChainUntilTargetUseBinarySchema(
          diagnostics, sizeOfAttribute.AccessChainToOtherMember);

      if (memberTypeInfo is IIntegerTypeInfo &&
          memberType is BinarySchemaStructureParser.PrimitiveMemberType
              primitiveMemberType) {
        primitiveMemberType.AccessChainToSizeOf =
            sizeOfAttribute.AccessChainToOtherMember;
      } else {
        diagnostics.Add(
            Rules.CreateDiagnostic(memberSymbol, Rules.NotSupported));
      }
    }
  }
}