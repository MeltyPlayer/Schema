using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

using schema.binary.parser;


namespace schema.binary.attributes {
  public class ChildOfParser {
    private readonly IList<Diagnostic> diagnostics_;

    public ChildOfParser(IList<Diagnostic> diagnostics) {
      this.diagnostics_ = diagnostics;
    }

    public INamedTypeSymbol? GetParentTypeSymbolOf(
        INamedTypeSymbol childNamedTypeSymbol) {
      if (!SymbolTypeUtil.ImplementsGeneric(childNamedTypeSymbol,
                                            typeof(IChildOf<>))) {
        return null;
      }

      var parentSymbol = childNamedTypeSymbol
                         .GetMembers(
                             nameof(IChildOf<IBinaryConvertible>.Parent))
                         .Single();
      return parentSymbol switch {
          IPropertySymbol propertySymbol => propertySymbol.Type,
          IFieldSymbol fieldSymbol       => fieldSymbol.Type,
      } as INamedTypeSymbol;
    }

    public void AssertParentContainsChild(
        INamedTypeSymbol parentNamedTypeSymbol,
        INamedTypeSymbol childNamedTypeSymbol) {
      var containedInClass =
          new TypeInfoParser()
              .ParseMembers(parentNamedTypeSymbol)
              .Where(tuple => tuple.Item2 is not IMethodSymbol)
              .Any(tuple => {
                var (parseStatus, _, memberTypeInfo) = tuple;
                if (parseStatus != TypeInfoParser.ParseStatus.SUCCESS) {
                  return false;
                }

                var elementTypeInfo =
                    (memberTypeInfo is ISequenceTypeInfo sequenceTypeInfo)
                        ? sequenceTypeInfo.ElementTypeInfo
                        : memberTypeInfo;
                var typeSymbol = elementTypeInfo.TypeSymbol;

                var hasSameName =
                    typeSymbol.Name == childNamedTypeSymbol.Name;
                var hasSameNamespace =
                    SymbolTypeUtil
                        .MergeContainingNamespaces(typeSymbol) ==
                    SymbolTypeUtil
                        .MergeContainingNamespaces(
                            childNamedTypeSymbol);
                var hasSameAssembly =
                    typeSymbol.ContainingAssembly ==
                    childNamedTypeSymbol.ContainingAssembly;

                if (hasSameName && hasSameNamespace &&
                    hasSameAssembly) {
                  return true;
                }

                return false;
              });

      if (!containedInClass) {
        diagnostics_.Add(
            Rules.CreateDiagnostic(childNamedTypeSymbol,
                                   Rules.ChildTypeMustBeContainedInParent));
      }
    }

    public void AssertParentBinaryConvertabilityMatchesChild(
        INamedTypeSymbol parentNamedTypeSymbol,
        INamedTypeSymbol childNamedTypeSymbol) {
      if ((childNamedTypeSymbol.IsBinaryDeserializable() &&
           !parentNamedTypeSymbol.IsBinaryDeserializable()) ||
          (childNamedTypeSymbol.IsBinarySerializable() &&
           !parentNamedTypeSymbol.IsBinarySerializable())) {
        this.diagnostics_.Add(
            Rules.CreateDiagnostic(
                childNamedTypeSymbol,
                Rules.ParentBinaryConvertabilityMustSatisfyChild));
      }
    }
  }
}