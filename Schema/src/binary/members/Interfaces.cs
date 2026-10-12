using schema.binary.parser;
using schema.util.symbols;

namespace schema.binary.members;

public interface ISchemaMemberEvaluator {
  bool TryToGetSchemaMember(
      IBetterSymbol memberSymbol,
      ITypeInfo typeInfo,
      out ISchemaMember schemaMember);
}