using Microsoft.CodeAnalysis;

using System.Linq;

using schema.util.enumerables;

namespace schema.util.symbols {
  public static class ParentExtensions {
    public static bool ContainsMemberWithType(this INamedTypeSymbol symbol,
                                              ISymbol other)
      => symbol.GetMembers()
               .Select(member => {
                         switch (member) {
                           case IFieldSymbol fieldSymbol:
                             return fieldSymbol.Type;
                           case IPropertySymbol propertySymbol:
                             return propertySymbol.Type;
                           default: return null;
                         }
                       })
               .Distinct(SymbolEqualityComparer.Default)
               .WhereNonnull()
               .Select(
                   memberSymbol
                       => memberSymbol.IsSequence(out var elementTypeV2, out _)
                           ? elementTypeV2
                           : memberSymbol)
               .Any(other.IsSameAs);
  }
}