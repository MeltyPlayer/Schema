﻿using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using INamedTypeSymbol = Microsoft.CodeAnalysis.INamedTypeSymbol;


namespace schema.util.generators {
  public abstract class BNamedTypesWithAttributeGenerator<TAttribute>
      : IIncrementalGenerator where TAttribute : Attribute {
    internal abstract bool FilterNamedTypesBeforeGenerating(
        TypeDeclarationSyntax syntax,
        INamedTypeSymbol symbol);

    internal abstract IEnumerable<(string fileName, string source)>
        GenerateSourcesForNamedType(INamedTypeSymbol symbol);

    public void Initialize(IncrementalGeneratorInitializationContext context) {
      var syntaxAndSymbolProvider
          = context.SyntaxProvider.CreateSyntaxProvider(
              (syntaxNode, _) => {
                if (!(syntaxNode is AttributeSyntax attributeSyntax &&
                      IsCorrectAttributeSyntax_(attributeSyntax))) {
                  return false;
                }

                return attributeSyntax.Parent?.Parent is TypeDeclarationSyntax;
              },
              (context, _) => {
                var syntax
                    = ((context.Node as AttributeSyntax)?.Parent?.Parent as
                        TypeDeclarationSyntax)!;
                var symbol
                    = (context.SemanticModel.GetDeclaredSymbol(syntax) as
                        INamedTypeSymbol)!;
                return (syntax, symbol);
              });

      var filteredSyntaxAndSymbolProvider = syntaxAndSymbolProvider.Where(
          syntaxAndSymbol
              => this.FilterNamedTypesBeforeGenerating(syntaxAndSymbol.syntax,
                syntaxAndSymbol.symbol));

      context.RegisterSourceOutput(
          filteredSyntaxAndSymbolProvider,
          (context, syntaxAndSymbol) => {
            foreach (var (fileName, source) in this.GenerateSourcesForNamedType(
                         syntaxAndSymbol.symbol)) {
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