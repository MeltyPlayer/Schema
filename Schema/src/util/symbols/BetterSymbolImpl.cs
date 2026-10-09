using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using schema.util.diagnostics;


namespace schema.util.symbols;

public static partial class BetterSymbol {
  public static IBetterSymbol<INamedTypeSymbol> FromType(
      INamedTypeSymbol symbol,
      SyntaxNodeAnalysisContext? context = null) {
    return new BetterSymbolImpl<INamedTypeSymbol>(symbol, context);
  }

  public static IBetterSymbol<INamedTypeSymbol> FromType(
      INamedTypeSymbol symbol,
      IDiagnosticReporter diagnosticReporter) {
    return new BetterSymbolImpl<INamedTypeSymbol>(symbol, diagnosticReporter);
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

    protected BetterSymbolImpl(ISymbol symbol,
                               IDiagnosticReporter diagnosticReporter) {
      this.Symbol = symbol;
      this.diagnosticReporter_ = diagnosticReporter;

      this.InitAttributes_();
    }

    public ISymbol Symbol { get; }
    public string Name => this.Symbol.Name;

    public IBetterSymbol<INamedTypeSymbol> GetContainingType()
      => new BetterSymbolImpl<INamedTypeSymbol>(
          this.Symbol.ContainingType,
          this.diagnosticReporter_.GetSubReporter(this.Symbol.ContainingType));

    public IBetterSymbol GetMember(ISymbol memberName)
      => new BetterSymbolImpl(
          memberName,
          this.diagnosticReporter_.GetSubReporter(memberName));
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