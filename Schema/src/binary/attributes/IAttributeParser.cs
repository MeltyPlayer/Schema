using System.Collections.Generic;

using Microsoft.CodeAnalysis;

using schema.binary.parser;

namespace schema.binary.attributes {
  internal interface IAttributeParser {
    void ParseIntoMemberType(IList<Diagnostic> diagnostics,
                             ISymbol memberSymbol,
                             ITypeInfo memberTypeInfo,
                             IMemberType memberType);
  }
}