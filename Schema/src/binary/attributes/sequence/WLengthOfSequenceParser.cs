using System.Linq;

using schema.binary.parser;
using schema.util.symbols;


namespace schema.binary.attributes {
  internal class WLengthOfSequenceParser : IAttributeParser {
    public void ParseIntoMemberType(IBetterSymbol memberBetterSymbol,
                                    ITypeInfo memberTypeInfo,
                                    IMemberType memberType) {
      var lengthOfSequenceAttributes =
          memberBetterSymbol.GetAttributes<WLengthOfSequenceAttribute>()
                            .ToArray();
      if (lengthOfSequenceAttributes.Length == 0) {
        return;
      }

      if (memberTypeInfo is IIntegerTypeInfo &&
          memberType is BinarySchemaContainerParser.PrimitiveMemberType
              primitiveMemberType) {
        primitiveMemberType.LengthOfSequenceMembers =
            lengthOfSequenceAttributes.Select(attr => attr.OtherMember)
                                      .ToArray();
      } else {
        memberBetterSymbol.ReportDiagnostic(Rules.NotSupported);
      }
    }
  }
}