using System;
using System.Collections.Immutable;
using System.Diagnostics;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;


namespace schema.binary {
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class BinarySchemaAnalyzer : DiagnosticAnalyzer {
    private readonly Type schemaAttributeType_ = typeof(BinarySchemaAttribute);
    private readonly BinarySchemaStructureParser parser_ = new();

    public override ImmutableArray<DiagnosticDescriptor>
        SupportedDiagnostics { get; } =
      ImmutableArray.Create(
          Rules.AllMembersInChainMustUseSchema,
          Rules.BooleanNeedsIntegerFormat,
          Rules.ChildTypeCanOnlyBeContainedInParent,
          Rules.ChildTypeMustBeContainedInParent,
          Rules.ConstUninitialized,
          Rules.ContainerTypeMustBePartial,
          Rules.DependentMustComeAfterSource,
          Rules.EnumNeedsIntegerFormat,
          Rules.ElementNeedsToImplementIBiSerializable,
          Rules.Exception,
          Rules.FormatOnNonNumber,
          Rules.IfBooleanNeedsNullable,
          Rules.MutableArrayNeedsLengthSource,
          Rules.MutableStringNeedsLengthSource,
          Rules.NotSupported,
          Rules.ReadAlreadyDefined,
          Rules.SchemaTypeMustBePartial,
          Rules.SourceMustBePrivate,
          Rules.StructureMemberNeedsToImplementIBiSerializable,
          Rules.UnexpectedAttribute,
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
        if (!SymbolTypeUtil.HasAttribute(symbol, this.schemaAttributeType_)) {
          return;
        }

        if (!SymbolTypeUtil.IsPartial(syntax)) {
          Rules.ReportDiagnostic(
              context,
              symbol,
              Rules.SchemaTypeMustBePartial);
          return;
        }

        var structure = this.parser_.ParseStructure(symbol);
        var diagnostics = structure.Diagnostics;
        if (diagnostics.Count > 0) {
          foreach (var diagnostic in diagnostics) {
            Rules.ReportDiagnostic(context, diagnostic);
          }
        }
      } catch (Exception exception) {
        if (Debugger.IsAttached) {
          throw;
        }
        Rules.ReportExceptionDiagnostic(context, symbol, exception);
      }
    }
  }
}