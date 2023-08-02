using System.Linq;

using schema.binary.parser;
using schema.util.symbols;


namespace schema.binary.attributes {
  internal class WLengthOfStringParser : IAttributeParser {
    public void ParseIntoMemberType(IBetterSymbol memberSymbol,
                                    ITypeInfo memberTypeInfo,
                                    IMemberType memberType) {
      var lengthOfStringAttributes =
          memberSymbol.GetAttributes<WLengthOfStringAttribute>()
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
        memberSymbol.ReportDiagnostic(Rules.NotSupported);
      }
    }
  }
}