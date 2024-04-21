using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using schema.binary;
using schema.binary.parser;


namespace schema.util.generators {
  public abstract class BNamedTypesWithAttributeGenerator<TAttribute>
      : IIncrementalGenerator where TAttribute : Attribute {
    internal abstract bool FilterNamedTypesBeforeGenerating(
        TypeDeclarationSyntax syntax,
        INamedTypeSymbol symbol);

    internal abstract IEnumerable<(string fileName, string source)>
        GenerateSourcesForNamedType(
            INamedTypeSymbol symbol,
            SemanticModel semanticModel,
            TypeDeclarationSyntax syntax);

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

      context.RegisterImplementationSourceOutput(
          context.CompilationProvider,
          (_, compilation) => {
            MemberReferenceUtil.PopulateBinaryTypes(compilation);
          });

      context.RegisterSourceOutput(
          filteredSyntaxAndSymbolProvider.Combine(context.CompilationProvider),
          (context, syntaxAndSymbolAndCompilation) => {
            var (syntaxAndSymbol, compilation) = syntaxAndSymbolAndCompilation;
            var (syntax, symbol) = syntaxAndSymbol;
            try {
              var semanticModel
                  = compilation.GetSemanticModel(syntax.SyntaxTree);
              foreach (var (fileName, source) in this
                           .GenerateSourcesForNamedType(
                               symbol,
                               semanticModel,
                               syntax)) {
                context.AddSource(fileName, source);
              }
            } catch (Exception e) {
              context.ReportDiagnostic(
                  Rules.CreateExceptionDiagnostic(symbol, e));
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