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
          if (memberSymbol is IPropertySymbol {IsIndexer: true}) {
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
           .Append(replacementName ?? namedTypeSymbol.Name.EscapeKeyword())
           .AppendGenericParametersWithVariance(namedTypeSymbol);

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

    public static IEnumerable<(ITypeParameterSymbol typeParameterSymbol,
        ITypeSymbol typeArgumentSymbol)> GetTypeParamsAndArgs(
        this INamedTypeSymbol symbol) {
      var typeParams = symbol.TypeParameters;
      var typeArgs = symbol.TypeArguments;
      for (var i = 0; i < typeParams.Length; ++i) {
        yield return (typeParams[i], typeArgs[i]);
      }
    }

    public static string GetGenericParametersWithVariance(
        this INamedTypeSymbol symbol)
      => new StringBuilder().AppendGenericParametersWithVariance(symbol)
                            .ToString();

    public static StringBuilder AppendGenericParametersWithVariance(
        this StringBuilder sb,
        INamedTypeSymbol symbol) {
      var typeParameters = symbol.TypeParameters;
      if (typeParameters.Length == 0) {
        return sb;
      }

      sb.Append("<");
      for (var i = 0; i < typeParameters.Length; ++i) {
        if (i > 0) {
          sb.Append(", ");
        }

        var typeParameter = typeParameters[i];
        sb.Append(typeParameter.Variance switch {
              VarianceKind.In   => "in ",
              VarianceKind.Out  => "out ",
              VarianceKind.None => "",
          })
          .Append(typeParameter.Name.EscapeKeyword());
      }

      sb.Append(">");
      return sb;
    }
  }
}