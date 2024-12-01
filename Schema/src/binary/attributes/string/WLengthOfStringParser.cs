using System.Linq;

using schema.binary.parser;
using schema.util.symbols;


namespace schema.binary.attributes;

internal class WLengthOfStringParser : IAttributeParser {
  public void ParseIntoMemberType(IBetterSymbol memberBetterSymbol,
                                  ITypeInfo memberTypeInfo,
                                  IMemberType memberType) {
    var lengthOfStringAttributes =
        memberBetterSymbol.GetAttributes<WLengthOfStringAttribute>()
                          .ToArray();
    if (lengthOfStringAttributes.Length == 0) {
      return;
    }

    if (memberTypeInfo is IIntegerTypeInfo &&
        memberType is BinarySchemaContainerParser.IntegerMemberType
            integerMemberType) {
      integerMemberType.LengthOfStringMembers =
          lengthOfStringAttributes.Select(attr => attr.OtherMember)
                                  .ToArray();
    } else {
      memberBetterSymbol.ReportDiagnostic(Rules.NotSupported);
    }
  }
}