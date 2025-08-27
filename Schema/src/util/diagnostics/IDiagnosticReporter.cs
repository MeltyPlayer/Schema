using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;


namespace schema.util.diagnostics;

public interface IDiagnosticReporter {
  void WithContext(SyntaxNodeAnalysisContext context);

  IDiagnosticReporter GetSubReporter(ISymbol childSymbol);

  void ReportDiagnostic(DiagnosticDescriptor diagnosticDescriptor);

  void ReportDiagnostic(ISymbol symbol,
                        DiagnosticDescriptor diagnosticDescriptor);

  void ReportException(Exception exception);

  IReadOnlyList<Diagnostic> Diagnostics { get; }
}