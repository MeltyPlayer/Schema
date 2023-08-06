using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using schema.util.diagnostics;
using schema.util.symbols;


namespace schema.binary.parser.asserts {
  internal class PartialContainerAsserter {
    private readonly IDiagnosticReporter diagnosticReporter_;

    public PartialContainerAsserter(IDiagnosticReporter diagnosticReporter) {
      this.diagnosticReporter_ = diagnosticReporter;
    }

    /// <summary>
    ///   All of the types that contain the given container need to be partial for the code generator to work.
    /// </summary>
    public void AssertContainersArePartial(INamedTypeSymbol containerSymbol) {
      var containingType = containerSymbol.ContainingType;
      while (containingType != null) {
        var typeDeclarationSyntax =
            containingType.DeclaringSyntaxReferences[0].GetSyntax() as
                TypeDeclarationSyntax;

        if (!SymbolTypeUtil.IsPartial(typeDeclarationSyntax!)) {
          this.diagnosticReporter_.ReportDiagnostic(
              containingType,
              Rules.ContainerTypeMustBePartial);
        }

        containingType = containingType.ContainingType;
      }
    }
  }
}