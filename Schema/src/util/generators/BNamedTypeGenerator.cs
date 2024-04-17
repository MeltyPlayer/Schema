using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using INamedTypeSymbol = Microsoft.CodeAnalysis.INamedTypeSymbol;


namespace schema.util.generators {
  public abstract class BNamedTypeGenerator<TAttribute>
      : IIncrementalGenerator {
    internal abstract bool Generate(
        INamedTypeSymbol typeSymbol,
        out string fileName,
        out string source);

    public void Initialize(IncrementalGeneratorInitializationContext context) {
      var symbolProvider
          = context.SyntaxProvider.CreateSyntaxProvider(
              (syntaxNode, _) => {
                if (!(syntaxNode is AttributeSyntax attributeSyntax &&
                      IsCorrectAttributeSyntax_(attributeSyntax))) {
                  return false;
                }

                return attributeSyntax.Parent?.Parent is TypeDeclarationSyntax;
              },
              (context, _) => {
                var typeDeclarationSyntax
                    = ((context.Node as AttributeSyntax)?.Parent?.Parent as
                        TypeDeclarationSyntax)!;
                var symbol
                    = (context.SemanticModel.GetDeclaredSymbol(
                        typeDeclarationSyntax) as INamedTypeSymbol)!;
                return symbol;
              });

      context.RegisterSourceOutput(
          symbolProvider,
          (context, symbol) => {
            if (this.Generate(symbol, out var fileName, out var source)) {
              context.AddSource(fileName, source);
            }
          });
    }

    private const string ATTRIBUTE_SUFFIX = "Attribute";

    private static bool IsCorrectAttributeSyntax_(
        AttributeSyntax syntax) {
      var syntaxName = syntax.Name.ToString();
      var typeName = typeof(TAttribute).Name;

      if (syntaxName == typeName) {
        return true;
      }

      return typeName.Length == syntaxName.Length + ATTRIBUTE_SUFFIX.Length &&
             typeName.StartsWith(syntaxName) &&
             typeName.EndsWith(ATTRIBUTE_SUFFIX);
    }
  }
}