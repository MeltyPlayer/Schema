using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using schema.binary;


namespace schema.util.diagnostics;

internal interface IDiagnosticReporter {
  void WithContext(SyntaxNodeAnalysisContext context);

  IDiagnosticReporter GetSubReporter(ISymbol childSymbol);

  void ReportDiagnostic(DiagnosticDescriptor diagnosticDescriptor);

  void ReportDiagnostic(ISymbol symbol,
                        DiagnosticDescriptor diagnosticDescriptor);

  void Report(Rule rule);
  void Report(ISymbol symbol, Rule rule);

  void ReportException(Exception exception);

  IReadOnlyList<Diagnostic> Diagnostics { get; }
}