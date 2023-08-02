using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using schema.binary;

namespace schema.util.diagnostics {
  internal class DiagnosticReporter : IDiagnosticReporter {
    private readonly List<Diagnostic> diagnostics_;

    public DiagnosticReporter(ISymbol symbol,
                              SyntaxNodeAnalysisContext? context = null)
        : this(symbol, context, new()) { }

    private DiagnosticReporter(ISymbol symbol,
                               SyntaxNodeAnalysisContext? context,
                               List<Diagnostic> diagnostics) {
      this.Symbol = symbol;
      this.Context = context;
      this.diagnostics_ = diagnostics;
    }

    public SyntaxNodeAnalysisContext? Context { get; }

    public ISymbol Symbol { get; }

    public IDiagnosticReporter GetSubReporter(ISymbol childSymbol)
      => new DiagnosticReporter(childSymbol, this.Context, this.diagnostics_);

    public void ReportDiagnostic(DiagnosticDescriptor diagnosticDescriptor)
      => this.ReportDiagnostic(this.Symbol, diagnosticDescriptor);

    public void ReportDiagnostic(ISymbol symbol,
                                 DiagnosticDescriptor diagnosticDescriptor)
      => this.ReportDiagnosticImpl_(
          Diagnostic.Create(diagnosticDescriptor,
                            symbol.Locations.First(),
                            symbol.Name));

    public void ReportException(Exception exception)
      => this.ReportDiagnosticImpl_(
          Diagnostic.Create(Rules.Exception,
                            this.Symbol.Locations.First(),
                            exception.Message,
                            exception.StackTrace.Replace("\r\n", "")
                                     .Replace("\n", "")));

    private void ReportDiagnosticImpl_(Diagnostic diagnostic) {
      this.diagnostics_.Add(diagnostic);
      this.Context?.ReportDiagnostic(diagnostic);
    }

    public IReadOnlyList<Diagnostic> Diagnostics => this.diagnostics_;
  }
}