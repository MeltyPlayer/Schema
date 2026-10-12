using schema.binary.parser;
using schema.util.symbols;

namespace schema.binary.members;

public class EnumSchemaMemberEvaluator : ISchemaMemberEvaluator {
  public bool TryToGetSchemaMember(
      IBetterSymbol memberSymbol,
      ITypeInfo typeInfo,
      out ISchemaMember schemaMember) {
    if (typeInfo is not IEnumTypeInfo enumTypeInfo) {
      schemaMember = null!;
      return false;
    }

    schemaMember = null!;
    return true;
  }
}