using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;


namespace schema.util.symbols {
  public static class TypeSymbolExtensions {
    public static bool IsArray(this ITypeSymbol typeSymbol,
                               out ITypeSymbol elementTypeSymbol) {
      if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol) {
        elementTypeSymbol = arrayTypeSymbol.ElementType;
        return true;
      }

      elementTypeSymbol = default;
      return false;
    }

    public static ITypeSymbol UnwrapNullable(this ITypeSymbol typeSymbol)
      => typeSymbol.IsNullable(out var nullableType)
          ? nullableType
          : typeSymbol;

    public static bool IsNullable(this ITypeSymbol typeSymbol,
                                  out ITypeSymbol nullableType) {
      if (typeSymbol.IsType(typeof(Nullable<>))) {
        nullableType = (typeSymbol as INamedTypeSymbol).TypeArguments[0];
        return true;
      }

      if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated) {
        nullableType
            = typeSymbol.WithNullableAnnotation(NullableAnnotation.None);
        return true;
      }

      nullableType = default;
      return false;
    }

    public static IEnumerable<INamedTypeSymbol> GetBaseTypes(
        this ISymbol symbol) {
      var baseType = (symbol as ITypeSymbol)?.BaseType;
      while (baseType != null) {
        yield return baseType;
        baseType = baseType.BaseType;
      }
    }

    public static ITypeSymbol AsNonNullable(this ITypeSymbol symbol)
      => symbol.IsNullable(out var nullableType) ? nullableType : symbol;
  }
}