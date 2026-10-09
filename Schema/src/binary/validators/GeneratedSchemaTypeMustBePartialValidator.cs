using Microsoft.CodeAnalysis;

using schema.util.symbols;

namespace schema.binary.validators;

public sealed class GeneratedSchemaTypeMustBePartialValidator
    : IContainerValidator {
  public static readonly Rule SCHEMA_TYPE_MUST_BE_PARTIAL_RULE
      = new("Type requesting generated Schema logic must be partial",
            "Schema type '{0}' must be partial to accept generated code.");

  public static readonly Rule WRAPPING_TYPE_MUST_BE_PARTIAL_RULE
      = new(
          "Containing types above type requesting generated Schema logic must be partial",
          "Type '{0}' contains a schema type, so it needs to be partial to accept generated code.");

  public void Validate(IBetterSymbol<INamedTypeSymbol> containerSymbol) {
    if (!containerSymbol.HasAttribute<BinarySchemaAttribute>()) {
      return;
    }

    if (!containerSymbol.TypedSymbol.IsPartial()) {
      containerSymbol.Report(SCHEMA_TYPE_MUST_BE_PARTIAL_RULE);
    }

    var currentContainingSymbol = containerSymbol.GetContainingType();
    while (currentContainingSymbol != null) {
      if (!containerSymbol.TypedSymbol.IsPartial()) {
        containerSymbol.Report(WRAPPING_TYPE_MUST_BE_PARTIAL_RULE);
      }

      currentContainingSymbol = currentContainingSymbol.GetContainingType();
    }
  }
}