using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace schema.binary.parser.asserts {
  public class PartialContainerAsserter {
    private readonly IList<Diagnostic> diagnostics_;

    public PartialContainerAsserter(IList<Diagnostic> diagnostics) {
      this.diagnostics_ = diagnostics;
    }

    /// <summary>
    ///   All of the types that contain the given structure need to be partial for the code generator to work.
    /// </summary>
    public void AssertContainersArePartial(INamedTypeSymbol structureSymbol) {
      var containingType = structureSymbol.ContainingType;
      while (containingType != null) {
        var typeDeclarationSyntax =
            containingType.DeclaringSyntaxReferences[0].GetSyntax() as
                TypeDeclarationSyntax;

        if (!SymbolTypeUtil.IsPartial(typeDeclarationSyntax!)) {
          this.diagnostics_.Add(Rules.CreateDiagnostic(
                                    containingType,
                                    Rules.ContainerTypeMustBePartial));
        }

        containingType = containingType.ContainingType;
      }
    }
  }
}