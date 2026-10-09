using schema.binary.attributes;
using schema.binary.parser;

namespace schema.binary.validators;

internal sealed class StringAttributesCanOnlyBeUsedOnStringsValidator : IMemberValidator {
  public static readonly Rule STRING_LENGTH_SOURCE_CAN_ONLY_BE_USED_ON_STRINGS_RULE
      = new(
          $"{nameof(StringLengthSourceAttribute)} can only be used on strings",
          "Member '{0}' is not a string, so it cannot have a " + $"{nameof(StringLengthSourceAttribute)}.");

  public static readonly Rule NULL_TERMINATED_STRING_CAN_ONLY_BE_USED_ON_STRINGS_RULE
      = new(
          $"{nameof(NullTerminatedStringAttribute)} can only be used on strings",
          "Member '{0}' is not a string, so it cannot have a " + $"{nameof(NullTerminatedStringAttribute)}.");

  public void Validate(IBinarySchemaMemberV2 member) {
    if (member.TypeInfo.Kind == SchemaTypeKind.STRING) {
      return;
    }

    var betterSymbol = member.MemberSymbol;
    if (betterSymbol.HasAttribute<StringLengthSourceAttribute>()) {
      betterSymbol.Report(STRING_LENGTH_SOURCE_CAN_ONLY_BE_USED_ON_STRINGS_RULE);
    }
    if (betterSymbol.HasAttribute<NullTerminatedStringAttribute>()) {
      betterSymbol.Report(NULL_TERMINATED_STRING_CAN_ONLY_BE_USED_ON_STRINGS_RULE);
    }
  }
}