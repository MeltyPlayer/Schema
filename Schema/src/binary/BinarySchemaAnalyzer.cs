using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

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
    => field != null
        ? field
        : field =
            ValidatorManager
                .AllValidatorTypes
                .SelectMany(t => t.GetFields(BindingFlags.Static |
                                             BindingFlags.Public |
                                             BindingFlags.NonPublic)
                                  .Where(f => f.FieldType == typeof(Rule)))
                .OrderBy(f => f.Name)
                .Select(f => (Rule) f.GetValue(null))
                .Select(rule => {
                          rule.InitDescriptor();
                          return rule.DiagnosticDescriptor;
                        })
                .ToImmutableArray();

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
      if (Debugger.IsAttached) {
        throw;
      }

      Rules.ReportExceptionDiagnostic(context, symbol, exception);
    }
  }
}