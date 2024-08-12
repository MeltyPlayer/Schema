using System;
using System.Collections.Immutable;
using System.Diagnostics;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using schema.util.symbols;
using schema.util.syntax;


namespace schema.binary {
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class BinarySchemaAnalyzer : DiagnosticAnalyzer {
    private readonly BinarySchemaContainerParser parser_ = new();

    public override ImmutableArray<DiagnosticDescriptor>
        SupportedDiagnostics { get; } =
      ImmutableArray.Create(
          Rules.AllMembersInChainMustUseSchema,
          Rules.BooleanNeedsIntegerFormat,
          Rules.ChildTypeCanOnlyBeContainedInParent,
          Rules.ChildTypeMustBeContainedInParent,
          Rules.ConstUninitialized,
          Rules.ContainerMemberBinaryConvertabilityNeedsToSatisfyParent,
          Rules.ContainerTypeMustBePartial,
          Rules.DependentMustComeAfterSource,
          Rules.EnumNeedsIntegerFormat,
          Rules.ElementBinaryConvertabilityNeedsToSatisfyParent,
          Rules.Exception,
          Rules.FormatOnNonNumber,
          Rules.IfBooleanNeedsNullable,
          Rules.MutableArrayNeedsLengthSource,
          Rules.MutableStringNeedsLengthSource,
          Rules.NotSupported,
          Rules.ParentBinaryConvertabilityMustSatisfyChild,
          Rules.ReadAlreadyDefined,
          Rules.SchemaTypeMustBePartial,
          Rules.SourceMustBePrivate,
          Rules.SymbolException,
          Rules.UnexpectedAttribute,
          Rules.UnexpectedSequenceAttribute,
          Rules.UnsupportedArrayType,
          Rules.WriteAlreadyDefined
      );

    public override void Initialize(AnalysisContext context) {
      /*if (!Debugger.IsAttached) {
        Debugger.Launch();
      }*/

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

        this.parser_.ParseContainer(symbol);
      } catch (Exception exception) {
        if (Debugger.IsAttached) {
          throw;
        }
        Rules.ReportExceptionDiagnostic(context, symbol, exception);
      }
    }
  }
}