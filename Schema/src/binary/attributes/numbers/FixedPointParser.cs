using System.Linq;

using schema.binary.parser;
using schema.util.symbols;


namespace schema.binary.attributes;

internal class FixedPointParser : IAttributeParser {
  public void ParseIntoMemberType(IBetterSymbol memberBetterSymbol,
                                  ITypeInfo memberTypeInfo,
                                  IMemberType memberType) {
    var fixedPointAttribute
        = memberBetterSymbol.GetAttribute<FixedPointAttribute>();
    if (fixedPointAttribute == null) {
      return;
    }

    if (memberTypeInfo is INumberTypeInfo &&
        memberType is BinarySchemaContainerParser.FloatMemberType
            floatMemberType) {
      floatMemberType.FixedPointAttribute = fixedPointAttribute;
    } else {
      memberBetterSymbol.ReportDiagnostic(Rules.NotSupported);
    }
  }
}