using System.Linq;

using Microsoft.CodeAnalysis;

using schema.binary.parser;
using schema.util.diagnostics;
using schema.util.symbols;


namespace schema.binary.attributes {
  internal class ChildOfParser {
    private readonly IDiagnosticReporter? diagnosticReporter_;

    public ChildOfParser(IDiagnosticReporter? diagnosticReporter) {
      this.diagnosticReporter_ = diagnosticReporter;
    }

    public INamedTypeSymbol? GetParentTypeSymbolOf(
        INamedTypeSymbol childNamedTypeSymbol) {
      if (!childNamedTypeSymbol.ImplementsGeneric(typeof(IChildOf<>))) {
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
                var (parseStatus, _, _, memberTypeInfo) = tuple;
                if (parseStatus != TypeInfoParser.ParseStatus.SUCCESS) {
                  return false;
                }

                var elementTypeInfo =
                    (memberTypeInfo is ISequenceTypeInfo sequenceTypeInfo)
                        ? sequenceTypeInfo.ElementTypeInfo
                        : memberTypeInfo;
                var typeSymbol = elementTypeInfo.TypeV2;
                return typeSymbol.IsExactly(childNamedTypeSymbol);
              });

      if (!containedInClass) {
        this.diagnosticReporter_.ReportDiagnostic(
            childNamedTypeSymbol,
            Rules.ChildTypeMustBeContainedInParent);
      }
    }

    public void AssertParentBinaryConvertabilityMatchesChild(
        INamedTypeSymbol parentNamedTypeSymbol,
        INamedTypeSymbol childNamedTypeSymbol) {
      if ((childNamedTypeSymbol.IsBinaryDeserializable() &&
           !parentNamedTypeSymbol.IsBinaryDeserializable()) ||
          (childNamedTypeSymbol.IsBinarySerializable() &&
           !parentNamedTypeSymbol.IsBinarySerializable())) {
        this.diagnosticReporter_.ReportDiagnostic(
            childNamedTypeSymbol,
            Rules.ParentBinaryConvertabilityMustSatisfyChild);
      }
    }
  }
}