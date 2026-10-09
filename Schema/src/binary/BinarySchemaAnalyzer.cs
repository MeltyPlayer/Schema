using System;
using System.Collections.Immutable;
using System.Diagnostics;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using schema.binary.validators;
using schema.util.symbols;
using schema.util.syntax;


namespace schema.binary;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BinarySchemaAnalyzer : DiagnosticAnalyzer {
  private readonly BinarySchemaContainerParser parser_ = new();

  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    => ValidatorManager.AllDiagnosticDescriptors;

  public override void Initialize(AnalysisContext context) {
    context.RegisterSyntaxNodeAction(
        syntaxNodeContext => {
          var syntax = syntaxNodeContext.Node as ClassDeclarationSyntax;

          var symbol =
              syntaxNodeContext.SemanticModel.GetDeclaredSymbol(syntax!);
          if (symbol is not INamedTypeSymbol namedTypeSymbol) {
            return;
          }

          this.CheckType(syntaxNodeContext, syntax!, namedTypeSymbol);
        },
        SyntaxKind.ClassDeclaration);

    context.RegisterSyntaxNodeAction(
        syntaxNodeContext => {
          var syntax = syntaxNodeContext.Node as StructDeclarationSyntax;

          var symbol =
              syntaxNodeContext.SemanticModel.GetDeclaredSymbol(syntax!);
          if (symbol is not INamedTypeSymbol namedTypeSymbol) {
            return;
          }

          this.CheckType(syntaxNodeContext, syntax!, namedTypeSymbol);
        },
        SyntaxKind.StructDeclaration);

    context.RegisterSyntaxNodeAction(
        syntaxNodeContext => {
          var syntax = syntaxNodeContext.Node as RecordDeclarationSyntax;

          var symbol =
              syntaxNodeContext.SemanticModel.GetDeclaredSymbol(syntax!);
          if (symbol is not INamedTypeSymbol namedTypeSymbol) {
            return;
          }

          this.CheckType(syntaxNodeContext, syntax!, namedTypeSymbol);
        },
        SyntaxKind.RecordDeclaration,
        SyntaxKind.RecordStructDeclaration);

  }

  public void CheckType(
      SyntaxNodeAnalysisContext context,
      TypeDeclarationSyntax syntax,
      INamedTypeSymbol symbol) {
    try {
      if (!symbol.HasAttribute<BinarySchemaAttribute>()) {
        return;
      }

      if (!syntax.IsPartial()) {
        Rules.ReportDiagnostic(
            context,
            symbol,
            Rules.SchemaTypeMustBePartial);
        return;
      }

      this.parser_.ParseContainer(BetterSymbol.FromType(symbol, context));
    } catch (Exception exception) {
      Rules.ReportExceptionDiagnostic(context, symbol, exception);
    }
  }
}