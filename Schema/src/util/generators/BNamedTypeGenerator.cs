using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using schema.util.data;


namespace schema.util.generators {
  public abstract class BNamedTypeGenerator : ISourceGenerator {
    private readonly Queue<INamedTypeSymbol> symbolQueue_ = new();

    private readonly SourceFileDictionary sourceFileDictionary_ = new();

    internal abstract void Generate(INamedTypeSymbol typeSymbol,
                                    ISourceFileDictionary sourceFileDictionary);

    public void Initialize(GeneratorInitializationContext context)
      => context.RegisterForSyntaxNotifications(() => new CustomReceiver(this));

    private class CustomReceiver(BNamedTypeGenerator g)
        : ISyntaxContextReceiver {
      public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
        TypeDeclarationSyntax syntax;
        ISymbol symbol;
        if (context.Node is not TypeDeclarationSyntax classDeclarationSyntax) {
          return;
        }

        symbol = context.SemanticModel.GetDeclaredSymbol(
            classDeclarationSyntax);
        if (symbol is not INamedTypeSymbol namedTypeSymbol) {
          return;
        }

        g.symbolQueue_.Enqueue(namedTypeSymbol);
      }
    }

    public void Execute(GeneratorExecutionContext context) {
      while (this.symbolQueue_.TryDequeue(out var symbol)) {
        this.Generate(symbol, this.sourceFileDictionary_);
      }

      this.sourceFileDictionary_.SetHandler(context.AddSource);
    }
  }
}