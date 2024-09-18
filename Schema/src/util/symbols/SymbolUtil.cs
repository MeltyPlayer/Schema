using System.Text;

using Microsoft.CodeAnalysis;


namespace schema.util.symbols;

public static class SymbolUtil {
  public static string GetUniqueNameForGenerator(this ISymbol symbol) {
    var sb = new StringBuilder();

    var fullyQualifiedNamespace = symbol.GetFullyQualifiedNamespace();
    if (fullyQualifiedNamespace != null) {
      sb.Append(fullyQualifiedNamespace)
        .Append(".");
    }

    var declaringTypes = symbol.GetDeclaringTypesDownward();
    foreach (var declaringType in declaringTypes) {
      sb.Append(declaringType.Name)
        .Append('.');
    }

    sb.Append(symbol.Name)
      .Append("_")
      .Append(symbol.GetArity());

    return sb.ToString();
  }

  public static int GetArity(this ISymbol symbol)
    => (symbol as INamedTypeSymbol)?.TypeArguments.Length ?? 0;

  public static bool Exists(this ISymbol symbol)
    => symbol.Locations.Length > 1;
}