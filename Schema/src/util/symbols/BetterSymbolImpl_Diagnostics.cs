using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using schema.binary;
using schema.util.diagnostics;


namespace schema.util.symbols;

public partial class BetterSymbol {
  private partial class BetterSymbolImpl {
    private readonly IDiagnosticReporter diagnosticReporter_;

    public void WithContext(SyntaxNodeAnalysisContext context)
      => this.diagnosticReporter_.WithContext(context);

    public IDiagnosticReporter GetSubReporter(ISymbol childSymbol)
      => this.diagnosticReporter_.GetSubReporter(childSymbol);

    public void ReportDiagnostic(DiagnosticDescriptor diagnosticDescriptor)
      => this.diagnosticReporter_.ReportDiagnostic(diagnosticDescriptor);

    public void ReportDiagnostic(ISymbol symbol,
                                 DiagnosticDescriptor diagnosticDescriptor)
      => this.diagnosticReporter_.ReportDiagnostic(
          symbol,
          diagnosticDescriptor);

    public void Report(Rule rule)
      => this.ReportDiagnostic(this.Symbol, rule.DiagnosticDescriptor);

    public void Report(ISymbol symbol, Rule rule)
      => this.ReportDiagnostic(symbol, rule.DiagnosticDescriptor);

    public void ReportException(Exception exception)
      => this.diagnosticReporter_.ReportException(exception);

    public IReadOnlyList<Diagnostic> Diagnostics
      => this.diagnosticReporter_.Diagnostics;
  }
}