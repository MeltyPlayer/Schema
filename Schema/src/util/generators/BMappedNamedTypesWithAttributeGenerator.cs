using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using schema.binary;
using schema.binary.parser;


namespace schema.util.generators {
  public abstract class BMappedNamedTypesWithAttributeGenerator<
      TAttribute, TMapped>
      : IIncrementalGenerator where TAttribute : Attribute {
    public abstract bool TryToMap(TypeDeclarationSyntax syntax,
                                  INamedTypeSymbol typeSymbol,
                                  out TMapped mapped);

    public abstract void PreprocessAllMapped(
        IReadOnlyDictionary<INamedTypeSymbol, TMapped> secondaries);

    public abstract void PreprocessCompilation(
        Compilation compilation);

    public abstract IEnumerable<(string fileName, string source)>
        GenerateSourcesForMappedNamedType(TMapped mapped);

    public void Initialize(IncrementalGeneratorInitializationContext context) {
      context.RegisterImplementationSourceOutput(
          context.CompilationProvider,
          (_, compilation) => {
            PreprocessCompilation(compilation);
          });

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

      var mappedProvider =
          syntaxAndSymbolProvider
              .Select(
                  (syntaxAndSymbol, _) => {
                    var (syntax, symbol) = syntaxAndSymbol;
                    var success = this.TryToMap(syntax, symbol, out var mapped);
                    return (success, (symbol, mapped));
                  })
              .Where(successAndMapped => successAndMapped.success)
              .Select((successAndMapped, _) => successAndMapped.Item2);

      context.RegisterSourceOutput(
          mappedProvider.Collect(),
          (context, allMapped) => {
            var mappedBySymbol
                = allMapped.ToDictionary(pair => pair.symbol,
                                         pair => pair.mapped);

            try {
              this.PreprocessAllMapped(mappedBySymbol);

              foreach (var kvp in mappedBySymbol) {
                var mapped = kvp.Value;
                foreach (var (fileName, source) in this
                             .GenerateSourcesForMappedNamedType(mapped)) {
                  context.AddSource(fileName, source);
                }
              }
            } catch (Exception ex) {
              context.ReportDiagnostic(Rules.CreateExceptionDiagnostic(mappedBySymbol.Keys.First(), ex));
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