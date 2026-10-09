using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using schema.util.diagnostics;


namespace schema.util.symbols;

public static partial class BetterSymbol {
  private static readonly Dictionary<ISymbol, IBetterSymbol>
      CACHE_BY_SYMBOL_ = new();

  public static void ClearCache() => CACHE_BY_SYMBOL_.Clear();

  public static IBetterSymbol<INamedTypeSymbol> FromType(
      INamedTypeSymbol symbol,
      SyntaxNodeAnalysisContext? context = null) {
    if (CACHE_BY_SYMBOL_.TryGetValue(symbol, out var betterSymbol)) {
      return (betterSymbol as IBetterSymbol<INamedTypeSymbol>)!;
    }

    return new BetterSymbolImpl<INamedTypeSymbol>(symbol, context);
  }

  public static IBetterSymbol<INamedTypeSymbol> FromType(
      INamedTypeSymbol symbol,
      IDiagnosticReporter diagnosticReporter) {
    if (CACHE_BY_SYMBOL_.TryGetValue(symbol, out var betterSymbol)) {
      return (betterSymbol as IBetterSymbol<INamedTypeSymbol>)!;
    }

    return new BetterSymbolImpl<INamedTypeSymbol>(symbol, diagnosticReporter);
  }

  public static IBetterSymbol FromMember(
      ISymbol symbol,
      IDiagnosticReporter diagnosticReporter) {
    if (CACHE_BY_SYMBOL_.TryGetValue(symbol, out var betterSymbol)) {
      return betterSymbol;
    }

    return new BetterSymbolImpl(symbol, diagnosticReporter);
  }


  private partial class BetterSymbolImpl : IBetterSymbol {
    public BetterSymbolImpl(ISymbol symbol,
                            SyntaxNodeAnalysisContext? context = null) : this(
        symbol,
        new DiagnosticReporter(symbol, context)) { }

    public BetterSymbolImpl(ISymbol symbol,
                            IDiagnosticReporter diagnosticReporter) {
      this.Symbol = symbol;
      this.diagnosticReporter_ = diagnosticReporter;

      CACHE_BY_SYMBOL_[symbol] = this;

      this.InitAttributes_();
    }

    public ISymbol Symbol { get; }
    public string Name => this.Symbol.Name;

    public IBetterSymbol<INamedTypeSymbol>? GetContainingType() {
      var containingType = this.Symbol.ContainingType;
      if (containingType == null) {
        return null;
      }
      
      return BetterSymbol.FromType(
          containingType,
          this.diagnosticReporter_.GetSubReporter(containingType));
    }

    public IBetterSymbol GetMember(ISymbol member)
      => BetterSymbol.FromMember(
          member,
          this.diagnosticReporter_.GetSubReporter(member));
  }

  private class BetterSymbolImpl<TSymbol>
      : BetterSymbolImpl,
        IBetterSymbol<TSymbol>
      where TSymbol : ISymbol {
    public BetterSymbolImpl(
        TSymbol symbol,
        SyntaxNodeAnalysisContext? context = null) : base(
        symbol,
        context) {
      this.TypedSymbol = symbol;
    }

    public BetterSymbolImpl(
        TSymbol symbol,
        IDiagnosticReporter diagnosticReporter) : base(
        symbol,
        diagnosticReporter) {
      this.TypedSymbol = symbol;
    }

    public TSymbol TypedSymbol { get; }
  }
}

public static class BetterSymbolExtensions {
  extension(IBetterSymbol<ITypeSymbol> betterSymbol) {
    public IBetterSymbol GetMember(string childName)
      => betterSymbol.GetMember(betterSymbol.TypedSymbol.GetMembers(childName)
                                            .Single());
  }
}