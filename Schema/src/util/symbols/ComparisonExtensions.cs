using System;
using System.Linq;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;

using schema.util.enumerables;


namespace schema.util.symbols;

public static class ComparisonExtensions {
  public static bool IsSameAs(this ISymbol symbol, ISymbol other)
    => symbol.Name == other.Name &&
       symbol.GetFullyQualifiedNamespace() ==
       other.GetFullyQualifiedNamespace() &&
       symbol.GetArity() == other.GetArity();

  public static bool IsType<T>(this ISymbol symbol)
    => symbol.IsInSameNamespaceAs<T>() &&
       symbol.Name == typeof(T).Name &&
       symbol.GetArity() == typeof(T).GenericTypeArguments.Length;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsType(this ISymbol symbol, Type expectedType) {
    var expectedName = expectedType.Name;

    int expectedArity = 0;
    var indexOfBacktick = expectedName.IndexOf('`');
    if (indexOfBacktick != -1) {
      expectedArity = int.Parse(expectedName.Substring(indexOfBacktick + 1));
      expectedName = expectedName.Substring(0, indexOfBacktick);
    }

    return symbol.Name == expectedName &&
           symbol.IsInSameNamespaceAs(expectedType) &&
           symbol.GetArity() == expectedArity;
  }

  public static bool Implements<T>(this ISymbol symbol)
    => symbol.Implements<T>(out _);

  public static bool Implements<T>(this ISymbol symbol,
                                   out INamedTypeSymbol matchingType)
    => symbol.Implements(typeof(T), out matchingType);

  public static bool Implements(this ISymbol symbol, Type type)
    => symbol.Implements(type, out _);

  public static bool Implements(this ISymbol symbol,
                                Type type,
                                out INamedTypeSymbol matchingType) {
    matchingType = symbol.Yield()
                         .Concat((symbol as ITypeSymbol)?.AllInterfaces ??
                                 Enumerable.Empty<ITypeSymbol>())
                         .Concat(symbol.GetBaseTypes())
                         .SingleOrDefault(symbol => symbol.IsType(type)) as
        INamedTypeSymbol;
    return matchingType != null;
  }
}