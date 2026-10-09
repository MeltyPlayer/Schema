using schema.binary.attributes;
using schema.binary.parser;
using schema.util.symbols;

namespace schema.binary.validators;

public sealed class RequiredStringAttributeValidator
    : IMemberValidator {
  public static readonly Rule STRING_CAN_ONLY_HAVE_ONE_REQUIRED_ATTRIBUTE_RULE
      = new(
          $"Strings can only have one of {nameof(StringLengthSourceAttribute)}, {nameof(RStringLengthSourceAttribute)}, or {nameof(NullTerminatedStringAttribute)}",
          $"String member '{{0}}' can only have one of {nameof(StringLengthSourceAttribute)}, {nameof(RStringLengthSourceAttribute)}, and {nameof(NullTerminatedStringAttribute)}.");

  public static readonly Rule STRING_REQUIRES_ATTRIBUTE_RULE
      = new(
          $"Strings need a {nameof(StringLengthSourceAttribute)}, {nameof(RStringLengthSourceAttribute)}, or {nameof(NullTerminatedStringAttribute)}",
          $"String member '{{0}}' must have a {nameof(StringLengthSourceAttribute)}, {nameof(RStringLengthSourceAttribute)}, or {nameof(NullTerminatedStringAttribute)}.");

  public static readonly Rule READONLY_STRING_CANNOT_HAVE_LENGTH_ATTRIBUTE_RULE
      = new(
          $"Readonly strings cannot have a {nameof(StringLengthSourceAttribute)}, {nameof(RStringLengthSourceAttribute)}, or {nameof(NullTerminatedStringAttribute)}",
          $"String member '{{0}}' is readonly, so it cannot have a {nameof(StringLengthSourceAttribute)}, {nameof(RStringLengthSourceAttribute)}, or {nameof(NullTerminatedStringAttribute)}.");

  public void Validate(IBetterSymbol memberSymbol, ITypeInfo typeInfo) {
    if (typeInfo.Kind != SchemaTypeKind.STRING) {
      return;
    }

    var attributeCount
        = (memberSymbol.HasAttribute<StringLengthSourceAttribute>() ? 1 : 0) +
          (memberSymbol.HasAttribute<RStringLengthSourceAttribute>() ? 1 : 0) +
          (memberSymbol.HasAttribute<NullTerminatedStringAttribute>() ? 1 : 0);
    if (typeInfo.IsReadOnly) {
      if (attributeCount >= 1) {
        memberSymbol.Report(READONLY_STRING_CANNOT_HAVE_LENGTH_ATTRIBUTE_RULE);
      }
    } else {
      if (attributeCount > 1) {
        memberSymbol.Report(STRING_CAN_ONLY_HAVE_ONE_REQUIRED_ATTRIBUTE_RULE);
      } else if (attributeCount == 0) {
        memberSymbol.Report(STRING_REQUIRES_ATTRIBUTE_RULE);
      }
    }
  }
}