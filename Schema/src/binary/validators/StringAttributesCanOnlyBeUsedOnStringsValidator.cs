using schema.binary.attributes;
using schema.binary.parser;
using schema.util.symbols;

namespace schema.binary.validators;

public sealed class StringAttributesCanOnlyBeUsedOnStringsValidator : IMemberValidator {
  public static readonly Rule STRING_ENCODING_CAN_ONLY_BE_USED_ON_STRINGS_RULE
      = new(
          $"{nameof(StringEncodingAttribute)} can only be used on strings",
          $"Member '{{0}}' is not a string, so it cannot have a {nameof(StringEncodingAttribute)}.");
  
  public static readonly Rule STRING_LENGTH_SOURCE_CAN_ONLY_BE_USED_ON_STRINGS_RULE
      = new(
          $"{nameof(StringLengthSourceAttribute)} can only be used on strings",
          $"Member '{{0}}' is not a string, so it cannot have a {nameof(StringLengthSourceAttribute)}.");

  public static readonly Rule RSTRING_LENGTH_SOURCE_CAN_ONLY_BE_USED_ON_STRINGS_RULE
      = new(
          $"{nameof(RStringLengthSourceAttribute)} can only be used on strings",
          $"Member '{{0}}' is not a string, so it cannot have a {nameof(RStringLengthSourceAttribute)}.");

  public static readonly Rule NULL_TERMINATED_STRING_CAN_ONLY_BE_USED_ON_STRINGS_RULE
      = new(
          $"{nameof(NullTerminatedStringAttribute)} can only be used on strings",
          $"Member '{{0}}' is not a string, so it cannot have a {nameof(NullTerminatedStringAttribute)}.");

  public void Validate(IBetterSymbol memberSymbol, ITypeInfo typeInfo) {
    if (typeInfo.Kind == SchemaTypeKind.STRING) {
      return;
    }

    if (memberSymbol.HasAttribute<StringEncodingAttribute>()) {
      memberSymbol.Report(STRING_ENCODING_CAN_ONLY_BE_USED_ON_STRINGS_RULE);
    }
    if (memberSymbol.HasAttribute<StringLengthSourceAttribute>()) {
      memberSymbol.Report(STRING_LENGTH_SOURCE_CAN_ONLY_BE_USED_ON_STRINGS_RULE);
    }
    if (memberSymbol.HasAttribute<RStringLengthSourceAttribute>()) {
      memberSymbol.Report(RSTRING_LENGTH_SOURCE_CAN_ONLY_BE_USED_ON_STRINGS_RULE);
    }
    if (memberSymbol.HasAttribute<NullTerminatedStringAttribute>()) {
      memberSymbol.Report(NULL_TERMINATED_STRING_CAN_ONLY_BE_USED_ON_STRINGS_RULE);
    }
  }
}