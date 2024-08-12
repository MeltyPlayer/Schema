using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using schema.util.diagnostics;


namespace schema.util.symbols {
  internal static partial class BetterSymbol {
    public static IBetterSymbol<INamedTypeSymbol> FromType(
        INamedTypeSymbol symbol,
        SyntaxNodeAnalysisContext? context = null) {
      return new BetterSymbolImpl<INamedTypeSymbol>(symbol, context);
    }

    public static IBetterSymbol FromMember(
        ISymbol symbol,
        SyntaxNodeAnalysisContext? context = null) {
      return new BetterSymbolImpl(symbol, context);
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
        return new BetterSymbolImpl(child,
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