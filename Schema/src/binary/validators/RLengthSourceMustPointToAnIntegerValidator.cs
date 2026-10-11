using schema.binary.attributes;
using schema.binary.parser;
using schema.util.symbols;

namespace schema.binary.validators;

public sealed class RLengthSourceMustPointToAnIntegerValidator
    : IMemberValidator {
  public static readonly Rule
      RSEQUENCE_LENGTH_SOURCE_MUST_POINT_TO_AN_INTEGER_RULE
          = new(
              $"{nameof(RSequenceLengthSourceAttribute)} must point to an integer",
              $"Sequence member '{{0}}''s {nameof(RSequenceLengthSourceAttribute)} must point to a member that is an integer.");

  public static readonly Rule
      RSTRING_LENGTH_SOURCE_MUST_POINT_TO_AN_INTEGER_RULE
          = new(
              $"{nameof(RStringLengthSourceAttribute)} must point to an integer",
              $"String member '{{0}}''s {nameof(RStringLengthSourceAttribute)} must point to a member that is an integer.");

  public void Validate(IBetterSymbol memberSymbol, ITypeInfo typeInfo) {
    if (memberSymbol.HasAttribute<SkipAttribute>()) {
      return;
    }

    if (typeInfo.Kind == SchemaTypeKind.SEQUENCE &&
        memberSymbol.TryGetAttribute<RSequenceLengthSourceAttribute>(
            out var rSequenceLengthSourceAttribute) &&
        !rSequenceLengthSourceAttribute.OtherMember.IsInteger) {
      memberSymbol.Report(
          RSEQUENCE_LENGTH_SOURCE_MUST_POINT_TO_AN_INTEGER_RULE);
    }

    if (typeInfo.Kind == SchemaTypeKind.STRING &&
        memberSymbol.TryGetAttribute<RStringLengthSourceAttribute>(
            out var rStringLengthSourceAttribute) &&
        !rStringLengthSourceAttribute.OtherMember.IsInteger) {
      memberSymbol.Report(
          RSTRING_LENGTH_SOURCE_MUST_POINT_TO_AN_INTEGER_RULE);
    }
  }
}