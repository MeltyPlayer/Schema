using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using schema.util.data;


namespace schema.util.generators {
  internal abstract class BNamedTypeSecondaryGenerator<TSecondary> : ISourceGenerator {
    private readonly Queue<(INamedTypeSymbol, TypeDeclarationSyntax)>
        symbolSyntaxQueue_ = new();

    private readonly SourceFileDictionary sourceFileDictionary_ = new();

    internal abstract bool TryToMapToSecondary(
        TypeDeclarationSyntax syntax,
        INamedTypeSymbol typeSymbol,
        out TSecondary secondary);

    internal abstract void PreprocessSecondaries(
        IReadOnlyDictionary<INamedTypeSymbol, TSecondary> secondaries);

    internal abstract void Generate(
        TSecondary secondary,
        ISourceFileDictionary sourceFileDictionary);

    public void Initialize(GeneratorInitializationContext context)
      => context.RegisterForSyntaxNotifications(() => new CustomReceiver(this));

    private class CustomReceiver : ISyntaxContextReceiver {
      private readonly BNamedTypeSecondaryGenerator<TSecondary> g_;

      public CustomReceiver(BNamedTypeSecondaryGenerator<TSecondary> g) {
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

        this.g_.symbolSyntaxQueue_.Enqueue((namedTypeSymbol, syntax));
      }
    }

    public void Execute(GeneratorExecutionContext context) {
      var secondaries = new Dictionary<INamedTypeSymbol, TSecondary>();
      while (this.symbolSyntaxQueue_.TryDequeue(out var namedTypeSymbol,
                                                out var syntax)) {
        try {
          if (this.TryToMapToSecondary(syntax,
                                       namedTypeSymbol,
                                       out var secondary)) {
            secondaries[namedTypeSymbol] = secondary;
          }
        } catch (Exception e) { }
      }

      this.PreprocessSecondaries(secondaries);

      foreach (var kvp in secondaries) {
        var secondary = kvp.Value;
        this.Generate(secondary, this.sourceFileDictionary_);
      }

      this.sourceFileDictionary_.SetHandler(context.AddSource);
    }
  }
}