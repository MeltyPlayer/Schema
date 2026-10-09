using System;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;


namespace schema.binary;

internal record Rule(string Title, string MessageFormat) {
  private DiagnosticDescriptor? impl_;

  public void InitDescriptor()
    => this.impl_
        ??= Rules.CreateDiagnosticDescriptor(this.Title, this.MessageFormat);

  public DiagnosticDescriptor DiagnosticDescriptor => this.impl_!;
}