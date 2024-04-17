using System;

using Microsoft.CodeAnalysis;

namespace schema.util.symbols {
  public static class SymbolComparisonUtil {
    public static bool Matches<T>(this ITypeSymbol symbol)
      => symbol.IsInSameNamespaceAs(typeof(T)) &&
         symbol.Name == typeof(T).Name;

    public static bool MatchesGeneric(this ITypeSymbol symbol,
                                      Type genericType)
      => symbol.IsInSameNamespaceAs(genericType) &&
         symbol.Name ==
         genericType.Name.Substring(0, genericType.Name.IndexOf('`'));
  }
}