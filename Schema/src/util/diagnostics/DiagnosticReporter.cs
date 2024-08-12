using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using schema.binary;


namespace schema.util.diagnostics {
  internal class DiagnosticReporter : IDiagnosticReporter {
    private readonly ISymbol symbol_;
    private SyntaxNodeAnalysisContext? context_;
    private readonly List<Diagnostic> diagnostics_;
    private List<Diagnostic>? unreportedDiagnostics_;

    public DiagnosticReporter(ISymbol symbol,
                              SyntaxNodeAnalysisContext? context = null)
        : this(symbol, context, new()) { }

    private DiagnosticReporter(ISymbol symbol,
                               SyntaxNodeAnalysisContext? context,
                               List<Diagnostic> diagnostics) {
      this.symbol_ = symbol;
      this.context_ = context;
      this.diagnostics_ = diagnostics;
      this.unreportedDiagnostics_ =
          context == null ? new List<Diagnostic>() : null;
    }

    public void WithContext(SyntaxNodeAnalysisContext context) {
      this.context_ = context;

      if (this.unreportedDiagnostics_ != null) {
        foreach (var diagnostic in this.unreportedDiagnostics_) {
          this.context_.Value.ReportDiagnostic(diagnostic);
        }

        this.unreportedDiagnostics_.Clear();
        this.unreportedDiagnostics_ = null;
      }
    }

    public IDiagnosticReporter GetSubReporter(ISymbol childSymbol)
      => new DiagnosticReporter(childSymbol, this.context_, this.diagnostics_);

    public void ReportDiagnostic(DiagnosticDescriptor diagnosticDescriptor)
      => this.ReportDiagnostic(this.symbol_, diagnosticDescriptor);

    public void ReportDiagnostic(ISymbol symbol,
                                 DiagnosticDescriptor diagnosticDescriptor)
      => this.ReportDiagnosticImpl_(
          Diagnostic.Create(diagnosticDescriptor,
                            symbol.Locations.First(),
                            symbol.Name));

    public void ReportException(Exception exception)
      => this.ReportDiagnosticImpl_(
          Diagnostic.Create(Rules.SymbolException,
                            this.symbol_.Locations.First(),
                            exception.Message,
                            exception.StackTrace.Replace("\r\n", "")
                                     .Replace("\n", "")));

    private void ReportDiagnosticImpl_(Diagnostic diagnostic) {
      this.diagnostics_.Add(diagnostic);
      if (this.context_ != null) {
        this.context_?.ReportDiagnostic(diagnostic);
      } else {
        this.unreportedDiagnostics_.Add(diagnostic);
      }
    }

    public IReadOnlyList<Diagnostic> Diagnostics => this.diagnostics_;
  }
}