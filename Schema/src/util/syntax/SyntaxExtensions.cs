using System.Linq;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using CSharpExtensions = Microsoft.CodeAnalysis.CSharpExtensions;

namespace schema.util.syntax {
  internal static class SyntaxExtensions {
    public static bool IsPartial(this TypeDeclarationSyntax syntax)
      => syntax.Modifiers.Any(
          m => CSharpExtensions.IsKind(m, SyntaxKind.PartialKeyword));
  }
}