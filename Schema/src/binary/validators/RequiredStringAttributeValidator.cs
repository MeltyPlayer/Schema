using schema.binary.attributes;
using schema.binary.parser;
using schema.util.symbols;

namespace schema.binary.validators;

internal sealed class RequiredStringAttributeValidator
    : IMemberValidator {
  public static readonly Rule STRING_CAN_ONLY_HAVE_ONE_REQUIRED_ATTRIBUTE_RULE
      = new(
          $"Strings can only have one of {nameof(StringLengthSourceAttribute)}, {nameof(RStringLengthSourceAttribute)}, or {nameof(NullTerminatedStringAttribute)}",
          $"String member '{{0}}' can not have {nameof(StringLengthSourceAttribute)}, {nameof(RStringLengthSourceAttribute)}, and {nameof(NullTerminatedStringAttribute)}.");

  public static readonly Rule STRING_REQUIRES_ATTRIBUTE_RULE
      = new(
          $"Strings need a {nameof(StringLengthSourceAttribute)}, {nameof(RStringLengthSourceAttribute)}, or {nameof(NullTerminatedStringAttribute)}",
          $"String member '{{0}}' must have a {nameof(StringLengthSourceAttribute)}, {nameof(RStringLengthSourceAttribute)}, or {nameof(NullTerminatedStringAttribute)}.");

  public void Validate(IBetterSymbol memberSymbol, ITypeInfo typeInfo) {
    if (typeInfo.Kind != SchemaTypeKind.STRING) {
      return;
    }

    var attributeCount
        = (memberSymbol.HasAttribute<StringLengthSourceAttribute>() ? 0 : 1) +
          (memberSymbol.HasAttribute<RStringLengthSourceAttribute>() ? 0 : 1) +
          (memberSymbol.HasAttribute<NullTerminatedStringAttribute>() ? 0 : 1);
    if (attributeCount > 1) {
      memberSymbol.Report(STRING_CAN_ONLY_HAVE_ONE_REQUIRED_ATTRIBUTE_RULE);
    } else if (attributeCount == 0) {
      memberSymbol.Report(STRING_REQUIRES_ATTRIBUTE_RULE);
    }
  }
}