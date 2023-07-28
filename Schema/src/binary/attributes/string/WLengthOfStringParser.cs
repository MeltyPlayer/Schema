using Microsoft.CodeAnalysis;

using System.Collections.Generic;

using schema.binary.parser;


namespace schema.binary.attributes {
  internal class WLengthOfStringParser : IAttributeParser {
    public void ParseIntoMemberType(IList<Diagnostic> diagnostics,
                                    ISymbol memberSymbol,
                                    ITypeInfo memberTypeInfo,
                                    IMemberType memberType) {
      var lengthOfStringAttribute =
          SymbolTypeUtil.GetAttribute<WLengthOfStringAttribute>(
              diagnostics,
              memberSymbol);
      if (lengthOfStringAttribute == null) {
        return;
      }

      if (memberTypeInfo is IIntegerTypeInfo &&
          memberType is BinarySchemaStructureParser.PrimitiveMemberType
              primitiveMemberType) {
        primitiveMemberType.LengthOfStringMember =
            lengthOfStringAttribute.OtherMember;
      } else {
        diagnostics.Add(
            Rules.CreateDiagnostic(memberSymbol, Rules.NotSupported));
      }
    }
  }
}