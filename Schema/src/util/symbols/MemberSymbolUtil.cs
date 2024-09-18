using System.Collections.Immutable;

using Microsoft.CodeAnalysis;


namespace schema.util.symbols;

public static class MemberSymbolUtil {
  public static bool IsPropertyGetter(this IMethodSymbol symbol,
                                      out string propertyName) {
    if (symbol.Name.StartsWith("get_")) {
      propertyName = symbol.Name.Substring(4);
      return true;
    }

    propertyName = default;
    return false;
  }

  public static bool IsIndexer(
      this IMethodSymbol symbol,
      out ImmutableArray<IParameterSymbol> parameterSymbols) {
    if (symbol is { AssociatedSymbol.Name: "this[]" } methodSymbol) {
      parameterSymbols = methodSymbol.Parameters;
      return true;
    }

    parameterSymbols = default;
    return false;
  }
}