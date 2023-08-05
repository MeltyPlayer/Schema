using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

using schema.util.enumerables;
using schema.util.symbols;

namespace schema.util.types {
  public static partial class TypeV2 {
    public static ITypeV2 FromSymbol(ISymbol symbol)
      => new SymbolTypeV2(symbol);

    private class SymbolTypeV2 : BSymbolTypeV2 {
      private readonly ISymbol symbol_;

      public SymbolTypeV2(ISymbol symbol) {
        this.symbol_ = symbol;
      }

      public override string Name => this.symbol_.Name;

      public override string FullyQualifiedNamespace
        => this.symbol_.GetFullyQualifiedNamespace();

      public override IEnumerable<string> NamespaceParts
        => this.symbol_.GetContainingNamespaces();

      public override bool Implements(Type type)
        => this.symbol_.Yield()
               .Concat((this.symbol_ as ITypeSymbol)?.AllInterfaces ??
                       Enumerable.Empty<ISymbol>())
               .Any(symbol => symbol.IsExactlyType(type));

      public override int GenericArgCount
        => (this.symbol_ as INamedTypeSymbol)?.TypeParameters.Length ?? 0;
    }
  }
}