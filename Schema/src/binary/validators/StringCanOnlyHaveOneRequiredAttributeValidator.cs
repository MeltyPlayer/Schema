using schema.binary.attributes;
using schema.binary.parser;

namespace schema.binary.validators;

internal sealed class StringCanOnlyHaveOneRequiredAttributeValidator
    : IMemberValidator {
  public static readonly Rule STRING_CAN_ONLY_HAVE_ONE_REQUIRED_ATTRIBUTE_RULE
      = new(
          $"Strings can only have one of {nameof(StringLengthSourceAttribute)} or {nameof(NullTerminatedStringAttribute)}",
          "String member '{0}'" +
          $" can not have both {nameof(StringLengthSourceAttribute)} and {nameof(NullTerminatedStringAttribute)}.");

  public void Validate(IBinarySchemaMemberV2 member) {
    if (member.TypeInfo.Kind != SchemaTypeKind.STRING) {
      return;
    }

    var betterSymbol = member.MemberSymbol;
    if (betterSymbol.HasAttribute<StringLengthSourceAttribute>() &&
        betterSymbol.HasAttribute<NullTerminatedStringAttribute>()) {
      betterSymbol.Report(STRING_CAN_ONLY_HAVE_ONE_REQUIRED_ATTRIBUTE_RULE);
    }
  }
}