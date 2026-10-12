using schema.binary.attributes;
using schema.binary.parser;
using schema.util.symbols;

namespace schema.binary.validators;

public sealed class FloatAttributesCanOnlyBeUsedOnFloatsValidator : IMemberValidator {
  public static readonly Rule FIXED_POINT_CAN_ONLY_BE_USED_ON_FLOATS_RULE
      = new(
          $"{nameof(FixedPointAttribute)} can only be used on floats",
          $"Member '{{0}}' is not a float, so it cannot have a {nameof(FixedPointAttribute)}.");
  
  public void Validate(IBetterSymbol memberSymbol, ITypeInfo typeInfo) {
    if (typeInfo.Kind == SchemaTypeKind.FLOAT) {
      return;
    }

    if (memberSymbol.HasAttribute<FixedPointAttribute>()) {
      memberSymbol.Report(FIXED_POINT_CAN_ONLY_BE_USED_ON_FLOATS_RULE);
    }
  }
}