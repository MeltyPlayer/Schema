using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace schema.util.symbols {
  public static class TypeSymbolUtil {
    public static bool IsArray(this ITypeSymbol typeSymbol,
                               out ITypeSymbol elementTypeSymbol) {
      if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol) {
        elementTypeSymbol = arrayTypeSymbol.ElementType;
        return true;
      }

      elementTypeSymbol = default;
      return false;
    }

    public static bool IsNullable(this ITypeSymbol typeSymbol)
      => typeSymbol.NullableAnnotation == NullableAnnotation.Annotated;

    public static IEnumerable<INamedTypeSymbol> GetBaseTypes(
        this ISymbol symbol) {
      var baseType = (symbol as ITypeSymbol)?.BaseType;
      while (baseType != null) {
        yield return baseType;
        baseType = baseType.BaseType;
      }
    }
  }
}