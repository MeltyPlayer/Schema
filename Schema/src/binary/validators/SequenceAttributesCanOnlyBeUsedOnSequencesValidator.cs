using schema.binary.attributes;
using schema.binary.parser;
using schema.util.symbols;

namespace schema.binary.validators;

public sealed class SequenceAttributesCanOnlyBeUsedOnSequencesValidator : IMemberValidator {
  public static readonly Rule SEQUENCE_LENGTH_SOURCE_CAN_ONLY_BE_USED_ON_SEQUENCES_RULE
      = new(
          $"{nameof(SequenceLengthSourceAttribute)} can only be used on sequences",
          $"Member '{{0}}' is not a sequence, so it cannot have a {nameof(SequenceLengthSourceAttribute)}.");

  public static readonly Rule RSEQUENCE_LENGTH_SOURCE_CAN_ONLY_BE_USED_ON_SEQUENCES_RULE
      = new(
          $"{nameof(RSequenceLengthSourceAttribute)} can only be used on sequences",
          $"Member '{{0}}' is not a sequence, so it cannot have a {nameof(RSequenceLengthSourceAttribute)}.");

  public static readonly Rule RSEQUENCE_UNTIL_END_OF_STREAM_CAN_ONLY_BE_USED_ON_SEQUENCES_RULE
      = new(
          $"{nameof(RSequenceUntilEndOfStreamAttribute)} can only be used on sequences",
          $"Member '{{0}}' is not a sequence, so it cannot have a {nameof(RSequenceUntilEndOfStreamAttribute)}.");

  public void Validate(IBetterSymbol memberSymbol, ITypeInfo typeInfo) {
    if (memberSymbol.HasAttribute<SkipAttribute>()) {
      return;
    }

    if (typeInfo.Kind == SchemaTypeKind.SEQUENCE) {
      return;
    }

    if (memberSymbol.HasAttribute<SequenceLengthSourceAttribute>()) {
      memberSymbol.Report(SEQUENCE_LENGTH_SOURCE_CAN_ONLY_BE_USED_ON_SEQUENCES_RULE);
    }
    if (memberSymbol.HasAttribute<RSequenceLengthSourceAttribute>()) {
      memberSymbol.Report(RSEQUENCE_LENGTH_SOURCE_CAN_ONLY_BE_USED_ON_SEQUENCES_RULE);
    }
    if (memberSymbol.HasAttribute<RSequenceUntilEndOfStreamAttribute>()) {
      memberSymbol.Report(RSEQUENCE_UNTIL_END_OF_STREAM_CAN_ONLY_BE_USED_ON_SEQUENCES_RULE);
    }
  }
}