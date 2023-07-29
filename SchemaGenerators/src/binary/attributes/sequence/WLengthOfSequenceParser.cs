using Microsoft.CodeAnalysis;

using System.Collections.Generic;

using schema.binary.parser;


namespace schema.binary.attributes {
  internal class WLengthOfSequenceParser : IAttributeParser {
    public void ParseIntoMemberType(IList<Diagnostic> diagnostics,
                                    ISymbol memberSymbol,
                                    ITypeInfo memberTypeInfo,
                                    IMemberType memberType) {
      var lengthOfSequenceAttribute =
          SymbolTypeUtil.GetAttribute<WLengthOfSequenceAttribute>(
              diagnostics,
              memberSymbol);
      if (lengthOfSequenceAttribute == null) {
        return;
      }

      if (memberTypeInfo is IIntegerTypeInfo &&
          memberType is BinarySchemaStructureParser.PrimitiveMemberType
              primitiveMemberType) {
        primitiveMemberType.LengthOfSequenceMember =
            lengthOfSequenceAttribute.OtherMember;
      } else {
        diagnostics.Add(
            Rules.CreateDiagnostic(memberSymbol, Rules.NotSupported));
      }
    }
  }
}