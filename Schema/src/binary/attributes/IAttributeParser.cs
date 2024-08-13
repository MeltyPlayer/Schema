using schema.binary.parser;
using schema.util.symbols;


namespace schema.binary.attributes;

internal interface IAttributeParser {
  void ParseIntoMemberType(IBetterSymbol memberBetterSymbol,
                           ITypeInfo memberTypeInfo,
                           IMemberType memberType);
}