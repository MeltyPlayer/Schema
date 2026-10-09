using schema.binary.attributes;
using schema.binary.parser;
using schema.util.symbols;

namespace schema.binary.validators;

internal sealed class IfBooleanNeedsNullableValidator
    : IMemberValidator {
  public static readonly Rule IF_BOOLEAN_NEEDS_NULLABLE_TYPE
      = new(
          $"{nameof(IfBooleanAttribute)} needs nullable type",
          $"Member '{{0}}' must be a nullable type to use {nameof(IfBooleanAttribute)}.");

  public void Validate(IBetterSymbol memberSymbol, ITypeInfo typeInfo) {
    if (!typeInfo.IsNullable &&
        (memberSymbol.HasAttribute<IfBooleanAttribute>() ||
         memberSymbol.HasAttribute<RIfBooleanAttribute>())) {
      memberSymbol.Report(IF_BOOLEAN_NEEDS_NULLABLE_TYPE);
    }
  }
}