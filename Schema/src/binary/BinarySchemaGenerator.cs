using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using schema.binary.attributes.size;
using schema.binary.text;
using schema.binary.util;


namespace schema.binary {
  [Generator(LanguageNames.CSharp)]
  internal class BinarySchemaGenerator : ISourceGenerator {
    private readonly Type schemaAttributeType_ = typeof(BinarySchemaAttribute);
    private readonly BinarySchemaStructureParser parser_ = new();

    private readonly BinarySchemaReaderGenerator readerImpl_ = new();
    private readonly BinarySchemaWriterGenerator writerImpl_ = new();

    private void Generate_(IBinarySchemaStructure structure) {
      if (structure.TypeSymbol.MemberNames.All(member => member != "Read")) {
        var readerCode = this.readerImpl_.Generate(structure);
        this.context_.Value.AddSource(
            SymbolTypeUtil.GetQualifiedName(structure.TypeSymbol) + "_reader.g",
            readerCode);
      }

      if (structure.TypeSymbol.MemberNames.All(member => member != "Write")) {
        var writerCode = this.writerImpl_.Generate(structure);
        this.context_.Value.AddSource(
            SymbolTypeUtil.GetQualifiedName(structure.TypeSymbol) + "_writer.g",
            writerCode);
      }
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
      private readonly BinarySchemaGenerator g_;

      public CustomReceiver(BinarySchemaGenerator g) {
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
        if (!SymbolTypeUtil.HasAttribute(symbol, this.schemaAttributeType_)) {
          return;
        }

        if (!SymbolTypeUtil.IsPartial(syntax)) {
          return;
        }

        var structure = this.parser_.ParseStructure(symbol);
        if (structure.Diagnostics.Count > 0) {
          return;
        }

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

      // Gathers up a map of all structures by named type symbol.
      var structureByNamedTypeSymbol =
          new Dictionary<INamedTypeSymbol, IBinarySchemaStructure>();
      foreach (var structure in this.queue_) {
        structureByNamedTypeSymbol[structure.TypeSymbol] = structure;
      }

      // Hooks up size of dependencies.
      {
        var sizeOfMemberInBytesDependencyFixer =
            new WSizeOfMemberInBytesDependencyFixer();
        foreach (var structure in this.queue_) {
          foreach (var member in structure.Members) {
            if (member.MemberType is IPrimitiveMemberType primitiveMemberType) {
              if (primitiveMemberType.AccessChainToSizeOf != null) {
                sizeOfMemberInBytesDependencyFixer.AddDependenciesForStructure(
                    structureByNamedTypeSymbol,
                    primitiveMemberType.AccessChainToSizeOf);
              }
              if (primitiveMemberType.AccessChainToPointer != null) {
                sizeOfMemberInBytesDependencyFixer.AddDependenciesForStructure(
                    structureByNamedTypeSymbol,
                    primitiveMemberType.AccessChainToPointer);
              }
            }
          }
        }
      }

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
    private readonly List<IBinarySchemaStructure> queue_ = new();

    public void EnqueueStructure(IBinarySchemaStructure structure) {
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