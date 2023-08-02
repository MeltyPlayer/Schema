using Microsoft.CodeAnalysis;

using schema.binary.parser;
using schema.util.diagnostics;

namespace schema.binary.attributes {
  internal interface IAttributeParser {
    void ParseIntoMemberType(IDiagnosticReporter diagnosticReporter,
                             ISymbol memberSymbol,
                             ITypeInfo memberTypeInfo,
                             IMemberType memberType);
  }
}