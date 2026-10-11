using schema.binary.attributes;
using schema.binary.parser;
using schema.util.symbols;

namespace schema.binary.validators;

public sealed class WLengthOfCanOnlyBeUsedOnIntegersValidator : IMemberValidator {
  public static readonly Rule
      WLENGTH_OF_SEQUENCE_CAN_ONLY_BE_USED_ON_INTEGERS_RULE
          = new(
              $"{nameof(WLengthOfSequenceAttribute)} can only be used on integers",
              $"Member '{{0}}' is not an integer, so it cannot be used with {nameof(WLengthOfSequenceAttribute)}.");
  
  public static readonly Rule
      WLENGTH_OF_STRING_CAN_ONLY_BE_USED_ON_INTEGERS_RULE
          = new(
              $"{nameof(WLengthOfStringAttribute)} can only be used on integers",
              $"Member '{{0}}' is not an integer, so it cannot be used with {nameof(WLengthOfStringAttribute)}.");

  public void Validate(IBetterSymbol memberSymbol, ITypeInfo typeInfo) {
    if (memberSymbol.HasAttribute<SkipAttribute>()) {
      return;
    }

    if (typeInfo.Kind == SchemaTypeKind.INTEGER) {
      return;
    }

    if (memberSymbol.HasAttribute<WLengthOfSequenceAttribute>()) {
      memberSymbol.Report(WLENGTH_OF_SEQUENCE_CAN_ONLY_BE_USED_ON_INTEGERS_RULE);
    }
    if (memberSymbol.HasAttribute<WLengthOfStringAttribute>()) {
      memberSymbol.Report(WLENGTH_OF_STRING_CAN_ONLY_BE_USED_ON_INTEGERS_RULE);
    }
  }
}