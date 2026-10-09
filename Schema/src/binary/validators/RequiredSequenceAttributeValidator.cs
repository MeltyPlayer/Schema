using schema.binary.attributes;
using schema.binary.parser;
using schema.util.symbols;

namespace schema.binary.validators;

public sealed class RequiredSequenceAttributeValidator
    : IMemberValidator {
  public static readonly Rule SEQUENCE_CAN_ONLY_HAVE_ONE_REQUIRED_ATTRIBUTE_RULE
      = new(
          $"Sequences can only have one of {nameof(SequenceLengthSourceAttribute)}, {nameof(RSequenceLengthSourceAttribute)}, or {nameof(RSequenceUntilEndOfStreamAttribute)}",
          $"Sequence member '{{0}}' can only have one of {nameof(SequenceLengthSourceAttribute)}, {nameof(RSequenceLengthSourceAttribute)}, and {nameof(RSequenceUntilEndOfStreamAttribute)}.");

  public static readonly Rule SEQUENCE_REQUIRES_ATTRIBUTE_RULE
      = new(
          $"Sequences need a {nameof(SequenceLengthSourceAttribute)}, {nameof(RSequenceLengthSourceAttribute)}, or {nameof(RSequenceUntilEndOfStreamAttribute)}",
          $"Sequence member '{{0}}' must have a {nameof(SequenceLengthSourceAttribute)}, {nameof(RSequenceLengthSourceAttribute)}, or {nameof(RSequenceUntilEndOfStreamAttribute)}.");

  public static readonly Rule CONST_LENGTH_SEQUENCE_CANNOT_HAVE_LENGTH_ATTRIBUTE_RULE
      = new(
          $"Const-length sequences cannot have a {nameof(SequenceLengthSourceAttribute)}, {nameof(RSequenceLengthSourceAttribute)}, or {nameof(RSequenceUntilEndOfStreamAttribute)}",
          $"Sequence member '{{0}}' has a const-length, so it cannot have a {nameof(SequenceLengthSourceAttribute)}, {nameof(RSequenceLengthSourceAttribute)}, or {nameof(RSequenceUntilEndOfStreamAttribute)}.");

  public void Validate(IBetterSymbol memberSymbol, ITypeInfo typeInfo) {
    if (memberSymbol.HasAttribute<SkipAttribute>()) {
      return;
    }

    if (typeInfo.Kind != SchemaTypeKind.SEQUENCE) {
      return;
    }

    var attributeCount
        = (memberSymbol.HasAttribute<SequenceLengthSourceAttribute>() ? 1 : 0) +
          (memberSymbol.HasAttribute<RSequenceLengthSourceAttribute>() ? 1 : 0) +
          (memberSymbol.HasAttribute<RSequenceUntilEndOfStreamAttribute>() ? 1 : 0);
    if (typeInfo is ISequenceTypeInfo { IsLengthConst: true }) {
      if (attributeCount >= 1) {
        memberSymbol.Report(CONST_LENGTH_SEQUENCE_CANNOT_HAVE_LENGTH_ATTRIBUTE_RULE);
      }
    } else {
      if (attributeCount > 1) {
        memberSymbol.Report(SEQUENCE_CAN_ONLY_HAVE_ONE_REQUIRED_ATTRIBUTE_RULE);
      } else if (attributeCount == 0) {
        memberSymbol.Report(SEQUENCE_REQUIRES_ATTRIBUTE_RULE);
      }
    }
  }
}