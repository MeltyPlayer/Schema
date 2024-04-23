using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;

using schema.binary;
using schema.util.sequences;

namespace schema.util.symbols {
  public static class SymbolUtil {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsInSameNamespaceAs<T>(this ISymbol symbol)
      => symbol.IsInSameNamespaceAs(typeof(T));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsInSameNamespaceAs(this ISymbol symbol, Type other)
      => symbol.IsInNamespace(other.Namespace);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsInNamespace(this ISymbol symbol,
                                       string fullNamespacePath) {
      var fullNamespacePathLength = fullNamespacePath.Length;

      var currentNamespace = symbol.ContainingNamespace;
      var currentNamespaceName = currentNamespace.Name;
      var currentNamespaceNameLength = currentNamespaceName.Length;

      if (fullNamespacePathLength == 0 && currentNamespaceNameLength == 0) {
        return true;
      }

      var fullNamespaceI = fullNamespacePathLength - 1;
      var currentNamespaceI = currentNamespaceNameLength - 1;

      for (; fullNamespaceI >= 0; --fullNamespaceI, --currentNamespaceI) {
        if (currentNamespaceI == -1) {
          if (fullNamespacePath[fullNamespaceI] != '.') {
            return false;
          }

          --fullNamespaceI;
          currentNamespace = currentNamespace.ContainingNamespace;
          currentNamespaceName = currentNamespace.Name;
          currentNamespaceNameLength = currentNamespaceName.Length;
          if (currentNamespaceNameLength == 0) {
            return false;
          }

          currentNamespaceI = currentNamespaceNameLength - 1;
        }

        if (fullNamespacePath[fullNamespaceI] !=
            currentNamespaceName[currentNamespaceI]) {
          return false;
        }
      }

      return fullNamespaceI == -1 &&
             currentNamespaceI == -1 &&
             currentNamespace.ContainingNamespace.Name.Length == 0;
    }


    public static string? GetFullyQualifiedNamespace(this ISymbol symbol) {
      var containingNamespaces = symbol.GetContainingNamespaces().ToArray();
      if (containingNamespaces.Length == 0) {
        return null;
      }

      return string.Join(".", containingNamespaces);
    }

    public static IEnumerable<string> GetContainingNamespaces(
        this ISymbol symbol)
      => symbol.GetContainingNamespacesReversed_().Reverse();

    private static IEnumerable<string> GetContainingNamespacesReversed_(
        this ISymbol symbol) {
      var namespaceSymbol = symbol.ContainingNamespace;
      if (namespaceSymbol?.IsGlobalNamespace ?? true) {
        yield break;
      }

      while (!namespaceSymbol?.IsGlobalNamespace ?? true) {
        if (namespaceSymbol.Name.Length > 0) {
          yield return namespaceSymbol.Name.EscapeKeyword();
        }

        namespaceSymbol = namespaceSymbol.ContainingNamespace;
      }
    }

    public static bool IsEnum(this ISymbol symbol,
                              out SchemaIntegerType underlyingType) {
      var underlyingSymbol = (symbol as INamedTypeSymbol)?.EnumUnderlyingType;
      if (underlyingSymbol == null) {
        underlyingType = default;
        return false;
      }

      var returnStatus = underlyingSymbol.IsPrimitive(out var primitiveType) &&
                         primitiveType != SchemaPrimitiveType.UNDEFINED;
      underlyingType = primitiveType.AsIntegerType();
      return returnStatus;
    }

    public static bool IsPrimitive(this ISymbol symbol,
                                   out SchemaPrimitiveType primitiveType) {
      var typeSymbol = symbol as ITypeSymbol;

      if (typeSymbol?.TypeKind == TypeKind.Enum) {
        primitiveType = SchemaPrimitiveType.ENUM;
        return true;
      }

      primitiveType = typeSymbol?.SpecialType switch {
          SpecialType.System_Boolean => SchemaPrimitiveType.BOOLEAN,
          SpecialType.System_Char    => SchemaPrimitiveType.CHAR,
          SpecialType.System_SByte   => SchemaPrimitiveType.SBYTE,
          SpecialType.System_Byte    => SchemaPrimitiveType.BYTE,
          SpecialType.System_Int16   => SchemaPrimitiveType.INT16,
          SpecialType.System_UInt16  => SchemaPrimitiveType.UINT16,
          SpecialType.System_Int32   => SchemaPrimitiveType.INT32,
          SpecialType.System_UInt32  => SchemaPrimitiveType.UINT32,
          SpecialType.System_Int64   => SchemaPrimitiveType.INT64,
          SpecialType.System_UInt64  => SchemaPrimitiveType.UINT64,
          SpecialType.System_Single  => SchemaPrimitiveType.SINGLE,
          SpecialType.System_Double  => SchemaPrimitiveType.DOUBLE,
          _                          => SchemaPrimitiveType.UNDEFINED
      };
      return primitiveType != SchemaPrimitiveType.UNDEFINED;
    }

    public static int GetArity(this ISymbol symbol)
      => (symbol as INamedTypeSymbol)?.TypeArguments.Length ?? 0;

    public static bool IsClass(this ISymbol symbol)
      => symbol is INamedTypeSymbol { TypeKind: TypeKind.Class };

    public static bool IsAbstractClass(this ISymbol symbol)
      => symbol is INamedTypeSymbol {
          IsAbstract: true, TypeKind: TypeKind.Class
      };

    public static bool IsInterface(this ISymbol symbol)
      => symbol is INamedTypeSymbol { TypeKind: TypeKind.Interface };

    public static bool IsStruct(this ISymbol symbol)
      => symbol is INamedTypeSymbol { TypeKind: TypeKind.Struct };

    public static bool IsString(this ISymbol symbol)
      => symbol is ITypeSymbol { SpecialType: SpecialType.System_String };

    public static bool IsGeneric(
        this ISymbol symbol,
        out ImmutableArray<ITypeParameterSymbol> typeParameterSymbols,
        out ImmutableArray<ITypeSymbol> typeArgumentSymbols) {
      if (symbol is INamedTypeSymbol { IsGenericType: true } namedTypeSymbol) {
        typeParameterSymbols = namedTypeSymbol.TypeParameters;
        typeArgumentSymbols = namedTypeSymbol.TypeArguments;
        return true;
      }

      typeParameterSymbols = default;
      typeArgumentSymbols = default;
      return false;
    }

    public static bool IsGenericTypeParameter(
        this ISymbol symbol,
        out ITypeParameterSymbol typeParameterSymbol) {
      if (symbol is not ITypeParameterSymbol tps) {
        typeParameterSymbol = default;
        return false;
      }

      typeParameterSymbol = tps;
      return true;
    }

    public static bool IsArray(this ISymbol symbol,
                               out ITypeSymbol elementType) {
      var arrayTypeSymbol = symbol as IArrayTypeSymbol;
      elementType = arrayTypeSymbol != null
          ? arrayTypeSymbol.ElementType
          : default;
      return arrayTypeSymbol != null;
    }

    public static bool IsTuple(this ISymbol symbol,
                               out IEnumerable<IFieldSymbol> tupleParameters) {
      if (symbol is INamedTypeSymbol { IsTupleType: true } namedTypeSymbol) {
        tupleParameters = namedTypeSymbol.TupleElements;
        return true;
      }

      tupleParameters = default;
      return false;
    }

    public static bool IsSequence(this ISymbol symbol,
                                  out ITypeSymbol elementType,
                                  out SequenceType sequenceType) {
      if (symbol.IsArray(out elementType)) {
        sequenceType = SequenceType.MUTABLE_ARRAY;
        return true;
      }

      if (symbol.Implements(typeof(ImmutableArray<>),
                            out var immutableArrayTypeV2)) {
        elementType = immutableArrayTypeV2.TypeArguments.ToArray()[0];
        sequenceType = SequenceType.IMMUTABLE_ARRAY;
        return true;
      }

      if (symbol.Implements(typeof(ISequence<,>), out var sequenceTypeV2)) {
        elementType = sequenceTypeV2.TypeArguments.ToArray()[1];
        sequenceType = SequenceType.MUTABLE_SEQUENCE;
        return true;
      }

      if (symbol.Implements(typeof(IConstLengthSequence<,>),
                            out var constLengthSequenceTypeV2)) {
        elementType = constLengthSequenceTypeV2.TypeArguments.ToArray()[1];
        sequenceType = SequenceType.MUTABLE_SEQUENCE;
        return true;
      }

      if (symbol.Implements(typeof(IReadOnlySequence<,>),
                            out var readOnlySequence)) {
        elementType = readOnlySequence.TypeArguments.ToArray()[1];
        sequenceType = SequenceType.READ_ONLY_SEQUENCE;
        return true;
      }

      if (symbol.Implements(typeof(List<>), out var listTypeV2)) {
        elementType = listTypeV2.TypeArguments.ToArray()[0];
        sequenceType = SequenceType.MUTABLE_LIST;
        return true;
      }

      if (symbol.Implements(typeof(IReadOnlyList<>),
                            out var readonlyListTypeV2)) {
        elementType = readonlyListTypeV2.TypeArguments.ToArray()[0];
        sequenceType = SequenceType.READ_ONLY_LIST;
        return true;
      }

      elementType = default;
      sequenceType = default;
      return false;
    }

    public static bool Exists(this ISymbol symbol)
      => symbol.Locations.Length > 1;
  }
}