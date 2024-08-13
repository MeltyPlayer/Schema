using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;


namespace schema.util.symbols;

public static class NamespaceExtensions {
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
}