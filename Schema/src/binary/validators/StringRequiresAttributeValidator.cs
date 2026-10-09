using schema.binary.attributes;
using schema.binary.parser;

namespace schema.binary.validators;

internal sealed class StringRequiresAttributeValidator : IMemberValidator {
  public static readonly Rule STRING_REQUIRES_ATTRIBUTE_RULE
      = new(
          $"Strings need a {nameof(StringLengthSourceAttribute)} or {nameof(NullTerminatedStringAttribute)}",
          "String member '{0}'" + $" must have either a {nameof(StringLengthSourceAttribute)} or {nameof(NullTerminatedStringAttribute)}.");

  public void Validate(IBinarySchemaMemberV2 member) {
    if (member.TypeInfo.Kind != SchemaTypeKind.STRING) {
      return;
    }

    var betterSymbol = member.MemberSymbol;
    if (!betterSymbol.HasAttribute<StringLengthSourceAttribute>() &&
        !betterSymbol.HasAttribute<NullTerminatedStringAttribute>()) {
      betterSymbol.Report(STRING_REQUIRES_ATTRIBUTE_RULE);
    }
  }
}