using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using schema.binary;
using schema.binary.util;

namespace schema.defaultinterface {
  [Generator(LanguageNames.CSharp)]
  internal class IncludeDefaultInterfaceMethodsGenerator : ISourceGenerator {
    private readonly Type attributeType_ =
        typeof(IncludeDefaultInterfaceMethodsAttribute);

    private readonly IncludeDefaultInterfaceMethodsParser parser_ = new();
    private readonly IncludeDefaultInterfaceMethodsWriter impl_ = new();

    private void Generate_(DefaultInterfaceMethodsData data) {
      var code = this.impl_.Generate(data);
      this.context_.Value.AddSource(
          SymbolTypeUtil.GetQualifiedName(data.StructureSymbol) + "_default_interface_methods.g",
          code);
    }

    public void Initialize(GeneratorInitializationContext context) {
#if DEBUG
      /*if (!Debugger.IsAttached) {
        Debugger.Launch();
      }*/
#endif

      context.RegisterForSyntaxNotifications(() => new CustomReceiver(this));
    }

    private class CustomReceiver : ISyntaxContextReceiver {
      private readonly IncludeDefaultInterfaceMethodsGenerator g_;

      public CustomReceiver(IncludeDefaultInterfaceMethodsGenerator g) {
        this.g_ = g;
      }

      public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
        TypeDeclarationSyntax syntax;
        ISymbol symbol;
        if (context.Node is ClassDeclarationSyntax classDeclarationSyntax) {
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

        this.g_.CheckType(context, syntax, namedTypeSymbol);
      }
    }

    public void CheckType(
        GeneratorSyntaxContext context,
        TypeDeclarationSyntax syntax,
        INamedTypeSymbol symbol) {
      try {
        if (!SymbolTypeUtil.HasAttribute(symbol, this.attributeType_)) {
          return;
        }

        if (!SymbolTypeUtil.IsPartial(syntax)) {
          return;
        }

        var structure = this.parser_.ParseStructure(symbol);

        this.EnqueueStructure(structure);
      } catch (Exception exception) {
        if (Debugger.IsAttached) {
          throw;
        }

        this.EnqueueError(symbol, exception);
      }
    }

    public void Execute(GeneratorExecutionContext context) {
      this.context_ = context;

      // Generates code for each structure.
      foreach (var structure in this.queue_) {
        try {
          this.Generate_(structure);
        } catch (Exception e) {
          ;
        }
      }

      this.queue_.Clear();

      foreach (var (errorSymbol, exception) in this.errorSymbols_) {
        this.context_.Value.ReportDiagnostic(
            Rules.CreateExceptionDiagnostic(errorSymbol, exception));
      }

      this.errorSymbols_.Clear();
    }

    private GeneratorExecutionContext? context_;
    private readonly List<DefaultInterfaceMethodsData> queue_ = new();

    public void EnqueueStructure(DefaultInterfaceMethodsData structure) {
      // If this assertion fails, then it means that syntax nodes are added
      // after the execution started.
      Asserts.Null(this.context_, "Syntax node added after execution!");
      this.queue_.Add(structure);
    }

    private readonly List<(ISymbol, Exception)> errorSymbols_ = new();

    public void EnqueueError(ISymbol errorSymbol, Exception exception) {
      // If this assertion fails, then it means that syntax nodes are added
      // after the execution started.
      Asserts.Null(this.context_, "Syntax node added after execution!");
      this.errorSymbols_.Add((errorSymbol, exception));
    }
  }
}