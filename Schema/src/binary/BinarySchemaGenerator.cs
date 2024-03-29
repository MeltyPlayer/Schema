﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using schema.binary.attributes;
using schema.binary.text;
using schema.util.asserts;
using schema.util.syntax;
using schema.util.types;

namespace schema.binary {
  [Generator(LanguageNames.CSharp)]
  internal class BinarySchemaGenerator : ISourceGenerator {
    private readonly BinarySchemaContainerParser parser_ = new();

    private readonly BinarySchemaReaderGenerator readerImpl_ = new();
    private readonly BinarySchemaWriterGenerator writerImpl_ = new();

    private void Generate_(IBinarySchemaContainer container) {
      var containerTypeV2 = TypeV2.FromSymbol(container.TypeSymbol);
      if (containerTypeV2.Implements<IBinaryDeserializable>()
          && container.TypeSymbol.MemberNames.All(member => member != "Read")) {
        var readerCode = this.readerImpl_.Generate(container);
        this.context_.Value.AddSource(
            $"{containerTypeV2.FullyQualifiedName}_reader.g",
            readerCode);
      }

      if (containerTypeV2.Implements<IBinarySerializable>()
          && container.TypeSymbol.MemberNames.All(
              member => member != "Write")) {
        var writerCode = this.writerImpl_.Generate(container);
        this.context_.Value.AddSource(
            $"{containerTypeV2.FullyQualifiedName}_writer.g",
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
      var typeV2 = TypeV2.FromSymbol(symbol);

      try {
        if (!typeV2.HasAttribute<BinarySchemaAttribute>()) {
          return;
        }

        if (!syntax.IsPartial()) {
          return;
        }

        var container = this.parser_.ParseContainer(symbol);
        if (container.Diagnostics.Count > 0) {
          return;
        }

        this.EnqueueContainer(container);
      } catch (Exception exception) {
        if (Debugger.IsAttached) {
          throw;
        }

        this.EnqueueError(symbol, exception);
      }
    }

    public void Execute(GeneratorExecutionContext context) {
      this.context_ = context;

      // Gathers up a map of all containers by named type symbol.
      var containerByNamedTypeSymbol =
          new ConcurrentDictionary<INamedTypeSymbol, IBinarySchemaContainer>(
              SymbolEqualityComparer.Default);
      foreach (var container in this.queue_) {
        containerByNamedTypeSymbol[container.TypeSymbol] = container;
      }

      // Hooks up size of dependencies.
      {
        var sizeOfMemberInBytesDependencyFixer =
            new WSizeOfMemberInBytesDependencyFixer();
        foreach (var container in this.queue_) {
          foreach (var member in
                   container.Members.OfType<ISchemaValueMember>()) {
            if (member.MemberType is IPrimitiveMemberType
                primitiveMemberType) {
              if (primitiveMemberType.AccessChainToSizeOf != null) {
                sizeOfMemberInBytesDependencyFixer.AddDependenciesForContainer(
                    containerByNamedTypeSymbol,
                    primitiveMemberType.AccessChainToSizeOf);
              }

              var pointerToAttribute = primitiveMemberType.PointerToAttribute;
              if (pointerToAttribute != null) {
                sizeOfMemberInBytesDependencyFixer.AddDependenciesForContainer(
                    containerByNamedTypeSymbol,
                    pointerToAttribute.AccessChainToOtherMember);
              }
            }
          }
        }
      }

      // Generates code for each container.
      foreach (var container in this.queue_) {
        try {
          this.Generate_(container);
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
    private readonly List<IBinarySchemaContainer> queue_ = new();

    public void EnqueueContainer(IBinarySchemaContainer container) {
      // If this assertion fails, then it means that syntax nodes are added
      // after the execution started.
      Asserts.Null(this.context_, "Syntax node added after execution!");
      this.queue_.Add(container);
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