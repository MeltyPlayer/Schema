using Microsoft.CodeAnalysis;

using System.Collections.Generic;
using System.Linq;

using schema.binary.parser;


namespace schema.binary.attributes {
  internal class WLengthOfStringParser : IAttributeParser {
    public void ParseIntoMemberType(IList<Diagnostic> diagnostics,
                                    ISymbol memberSymbol,
                                    ITypeInfo memberTypeInfo,
                                    IMemberType memberType) {
      var lengthOfStringAttributes =
          memberSymbol.GetAttributes<WLengthOfStringAttribute>(diagnostics)
                      .ToArray();
      if (lengthOfStringAttributes.Length == 0) {
        return;
      }

      if (memberTypeInfo is IIntegerTypeInfo &&
          memberType is BinarySchemaStructureParser.PrimitiveMemberType
              primitiveMemberType) {
        primitiveMemberType.LengthOfStringMembers =
            lengthOfStringAttributes.Select(attr => attr.OtherMember)
                                    .ToArray();
      } else {
        diagnostics.Add(
            Rules.CreateDiagnostic(memberSymbol, Rules.NotSupported));
      }
    }
  }
}