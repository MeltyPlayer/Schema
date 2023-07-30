using Microsoft.CodeAnalysis;

using System.Collections.Generic;
using System.Linq;

using schema.binary.parser;


namespace schema.binary.attributes {
  internal class WLengthOfSequenceParser : IAttributeParser {
    public void ParseIntoMemberType(IList<Diagnostic> diagnostics,
                                    ISymbol memberSymbol,
                                    ITypeInfo memberTypeInfo,
                                    IMemberType memberType) {
      var lengthOfSequenceAttributes =
          memberSymbol.GetAttributes<WLengthOfSequenceAttribute>(diagnostics)
                      .ToArray();
      if (lengthOfSequenceAttributes.Length == 0) {
        return;
      }

      if (memberTypeInfo is IIntegerTypeInfo &&
          memberType is BinarySchemaStructureParser.PrimitiveMemberType
              primitiveMemberType) {
        primitiveMemberType.LengthOfSequenceMembers =
            lengthOfSequenceAttributes.Select(attr => attr.OtherMember)
                                      .ToArray();
      } else {
        diagnostics.Add(
            Rules.CreateDiagnostic(memberSymbol, Rules.NotSupported));
      }
    }
  }
}