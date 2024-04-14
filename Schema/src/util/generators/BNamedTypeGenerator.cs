using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace schema.util.generators {
  public abstract class BNamedTypeGenerator : ISourceGenerator {
    private readonly SourceFileDictionary sourceFileDictionary_ = new();

    internal abstract void Generate(
        TypeDeclarationSyntax syntax,
        INamedTypeSymbol typeSymbol,
        ISourceFileDictionary sourceFileDictionary);

    public void Initialize(GeneratorInitializationContext context)
      => context.RegisterForSyntaxNotifications(() => new CustomReceiver(this));

    private class CustomReceiver : ISyntaxContextReceiver {
      private readonly BNamedTypeGenerator g_;

      public CustomReceiver(BNamedTypeGenerator g) {
        this.g_ = g;
      }

      public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
        TypeDeclarationSyntax syntax;
        ISymbol symbol;
        if (context.Node is TypeDeclarationSyntax classDeclarationSyntax) {
          syntax = classDeclarationSyntax;
        } else if (context.Node is StructDeclarationSyntax
                   structDeclarationSyntax) {
          syntax = structDeclarationSyntax;
        } else {
          return;
        }

        symbol = context.SemanticModel.GetDeclaredSymbol(syntax);
        if (symbol is not INamedTypeSymbol namedTypeSymbol) {
          return;
        }

        this.g_.Generate(syntax,
                         namedTypeSymbol,
                         this.g_.sourceFileDictionary_);
      }
    }

    public void Execute(GeneratorExecutionContext context)
      => this.sourceFileDictionary_.SetHandler(context.AddSource);
  }
}