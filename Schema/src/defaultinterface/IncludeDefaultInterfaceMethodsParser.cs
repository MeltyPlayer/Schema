using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace schema.defaultinterface {
  public class DefaultInterfaceMethodsData {
    public INamedTypeSymbol StructureSymbol { get; set; }

    // Will either be IMethodSymbol or IFieldSymbol
    public IReadOnlyList<ISymbol> AllMembersToInclude { get; set; }

    public IReadOnlyList<UsingDirectiveSyntax> AllUsingDirectives { get; set; }
  }

  public class IncludeDefaultInterfaceMethodsParser {
    public DefaultInterfaceMethodsData ParseStructure(
        INamedTypeSymbol structureSymbol) {
      var allMethodsToInclude = new HashSet<ISymbol>();
      var allUsingDirectives = new HashSet<UsingDirectiveSyntax>();

      var membersFromStructure = structureSymbol.GetMembers();

      var allInterfaces = structureSymbol.AllInterfaces;
      foreach (var anInterface in allInterfaces) {
        var methodsToIncludeFromInterface = new List<ISymbol>();

        foreach (var memberFromInterface in anInterface.GetMembers()) {
          if (memberFromInterface.Kind is not (SymbolKind.Property
                                               or SymbolKind.Method)) {
            continue;
          }

          if (memberFromInterface.IsAbstract) {
            continue;
          }

          foreach (var memberFromStructure in membersFromStructure) {
            if (memberFromStructure.Kind is not (SymbolKind.Property
                                                 or SymbolKind.Method)) {
              continue;
            }

            if (memberFromInterface.Name != memberFromStructure.Name &&
                !memberFromInterface.Name.EndsWith(
                    $".{memberFromStructure.Name}")) {
              goto DidNotMatch;
            }

            if (memberFromInterface.Kind != memberFromStructure.Kind) {
              goto DidNotMatch;
            }

            // TODO: Need to handle type??

            if (memberFromInterface is IPropertySymbol propertyFromInterface) {
              var propertyFromStructure = (IPropertySymbol) memberFromStructure;

              // TODO: Anything else to handle here??
            } else if
                (memberFromInterface is IMethodSymbol methodFromInterface) {
              var methodFromStructure = (IMethodSymbol) memberFromStructure;

              if (methodFromInterface.Arity != methodFromStructure.Arity) {
                goto DidNotMatch;
              }

              if (!methodFromStructure
                   .Parameters
                   .Select(param => param.Type.ToString())
                   .SequenceEqual(
                       methodFromInterface.Parameters.Select(
                           param => param.Type.ToString()))) {
                goto DidNotMatch;
              }
            }

            goto FoundMatch;

            DidNotMatch: ;
          }

          methodsToIncludeFromInterface.Add(memberFromInterface);

          FoundMatch: ;
        }

        if (methodsToIncludeFromInterface.Count == 0) {
          continue;
        }

        foreach (var method in methodsToIncludeFromInterface) {
          allMethodsToInclude.Add(method);
        }

        var interfaceUsingDirectives = anInterface
                                       .DeclaringSyntaxReferences.SelectMany(
                                           reference => reference.SyntaxTree
                                               .GetRoot()
                                               .DescendantNodesAndSelf())
                                       .OfType<UsingDirectiveSyntax>()
                                       .Distinct();
        foreach (var usingDirective in interfaceUsingDirectives) {
          allUsingDirectives.Add(usingDirective);
        }
      }

      return new DefaultInterfaceMethodsData {
          StructureSymbol = structureSymbol,
          AllMembersToInclude = allMethodsToInclude.ToArray(),
          AllUsingDirectives = allUsingDirectives.ToArray(),
      };
    }
  }
}