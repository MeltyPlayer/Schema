using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using schema.util.diagnostics;

namespace schema.util.symbols {
  internal static partial class BetterSymbol {
    private static Dictionary<INamedTypeSymbol, IBetterSymbol<INamedTypeSymbol>>
        typeCache_ = new(SymbolEqualityComparer.Default);

    private static Dictionary<ISymbol, IBetterSymbol> memberCache_ =
        new(SymbolEqualityComparer.Default);


    public static void ClearCache() {
      BetterSymbol.typeCache_.Clear();
      BetterSymbol.memberCache_.Clear();
    }

    public static IBetterSymbol<INamedTypeSymbol> FromType(
        INamedTypeSymbol symbol,
        SyntaxNodeAnalysisContext? context = null) {
      if (BetterSymbol.typeCache_.TryGetValue(symbol, out var betterSymbol)) {
        if (context != null) {
          betterSymbol.WithContext(context.Value);
        }

        return betterSymbol;
      }

      return BetterSymbol.typeCache_[symbol] =
          new BetterSymbolImpl<INamedTypeSymbol>(symbol, context);
    }

    public static IBetterSymbol FromMember(
        ISymbol symbol,
        SyntaxNodeAnalysisContext? context = null) {
      if (BetterSymbol.memberCache_.TryGetValue(symbol, out var betterSymbol)) {
        if (context != null) {
          betterSymbol.WithContext(context.Value);
        }

        return betterSymbol;
      }

      return BetterSymbol.memberCache_[symbol] =
          new BetterSymbolImpl(symbol, context);
    }


    private partial class BetterSymbolImpl : IBetterSymbol {
      public BetterSymbolImpl(ISymbol symbol,
                              SyntaxNodeAnalysisContext? context = null) : this(
          symbol,
          new DiagnosticReporter(symbol, context)) { }

      private BetterSymbolImpl(ISymbol symbol,
                               IDiagnosticReporter diagnosticReporter) {
        this.Symbol = symbol;
        this.diagnosticReporter_ = diagnosticReporter;
      }

      public ISymbol Symbol { get; }
      public string Name => this.Symbol.Name;

      public IBetterSymbol GetChild(ISymbol child) {
        if (BetterSymbol.memberCache_.TryGetValue(
                child,
                out var betterSymbol)) {
          return betterSymbol;
        }

        return BetterSymbol.memberCache_[child] = new BetterSymbolImpl(child,
          this.diagnosticReporter_
              .GetSubReporter(child));
      }
    }

    private class BetterSymbolImpl<TSymbol> : BetterSymbolImpl,
                                              IBetterSymbol<TSymbol>
        where TSymbol : ISymbol {
      public BetterSymbolImpl(TSymbol symbol,
                              SyntaxNodeAnalysisContext? context = null) : base(
          symbol,
          context) {
        this.TypedSymbol = symbol;
      }

      public TSymbol TypedSymbol { get; }
    }
  }
}