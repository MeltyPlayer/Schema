using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;


namespace schema.util.symbols {
  public static class NamedTypeSymbolUtil {
    public static IEnumerable<ISymbol> GetInstanceMembers(
        this INamedTypeSymbol containerSymbol) {
      var baseClassesAndSelf = new LinkedList<INamedTypeSymbol>();
      {
        var currentSymbol = containerSymbol;
        while (currentSymbol != null) {
          baseClassesAndSelf.AddFirst(currentSymbol);
          currentSymbol = currentSymbol.BaseType;
        }
      }

      foreach (var currentSymbol in baseClassesAndSelf) {
        foreach (var memberSymbol in currentSymbol.GetMembers()) {
          // Skips static/const fields
          if (memberSymbol.IsStatic) {
            continue;
          }

          // Skips backing field, these are used internally for properties
          if (memberSymbol.Name.Contains("k__BackingField")) {
            continue;
          }

          // Skips indexers.
          if (memberSymbol is IPropertySymbol { IsIndexer: true }) {
            continue;
          }

          yield return memberSymbol;
        }
      }
    }

    public static string GetQualifiersAndNameAndGenericParametersFor(
        this INamedTypeSymbol namedTypeSymbol,
        string? replacementName = null)
      => new StringBuilder()
         .AppendQualifiersAndNameAndGenericParametersFor(
             namedTypeSymbol,
             replacementName)
         .ToString();

    public static StringBuilder AppendQualifiersAndNameAndGenericParametersFor(
        this StringBuilder sb,
        INamedTypeSymbol namedTypeSymbol,
        string? replacementName = null)
      => sb.AppendSymbolQualifiers(namedTypeSymbol)
           .Append(" ")
           .AppendNameAndGenericParametersFor(namedTypeSymbol, replacementName);

    public static string GetNameAndGenericParametersFor(
        this INamedTypeSymbol namedTypeSymbol,
        string? replacementName = null)
      => new StringBuilder()
         .AppendNameAndGenericParametersFor(namedTypeSymbol, replacementName)
         .ToString();

    public static StringBuilder AppendNameAndGenericParametersFor(
        this StringBuilder sb,
        INamedTypeSymbol namedTypeSymbol,
        string? replacementName = null)
      => sb.Append(replacementName ?? namedTypeSymbol.Name.EscapeKeyword())
           .AppendGenericParametersFor(namedTypeSymbol);

    public static string GetGenericParameters(
        this INamedTypeSymbol namedTypeSymbol)
      => new StringBuilder()
         .AppendGenericParametersFor(namedTypeSymbol)
         .ToString();

    public static StringBuilder AppendGenericParametersFor(
        this StringBuilder sb,
        INamedTypeSymbol namedTypeSymbol)
      => sb.AppendGenericParameters(namedTypeSymbol.TypeParameters);
  }
}