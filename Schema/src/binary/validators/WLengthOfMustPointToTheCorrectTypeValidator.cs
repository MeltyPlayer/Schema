using System.Linq;

using schema.binary.attributes;
using schema.binary.parser;
using schema.util.symbols;

namespace schema.binary.validators;

public sealed class WLengthOfMustPointToTheCorrectTypeValidator
    : IMemberValidator {
  public static readonly Rule
      WLENGTH_OF_SEQUENCE_MUST_POINT_TO_A_SEQUENCE_RULE
          = new(
              $"{nameof(WLengthOfSequenceAttribute)} must point to a sequence",
              $"Member '{{0}}''s {nameof(WLengthOfSequenceAttribute)} points to a member that is not a sequence.");

  public static readonly Rule
      WLENGTH_OF_STRING_MUST_POINT_TO_A_STRING_RULE
          = new(
              $"{nameof(WLengthOfStringAttribute)} must point to a string",
              $"Member '{{0}}''s {nameof(WLengthOfStringAttribute)} points to a member that is not a string.");

  public void Validate(IBetterSymbol memberSymbol, ITypeInfo typeInfo) {
    if (memberSymbol.HasAttribute<SkipAttribute>()) {
      return;
    }

    if (memberSymbol.GetAttributes<WLengthOfSequenceAttribute>()
                    .Any(a => !a.OtherMember.IsSequence)) {
      memberSymbol.Report(WLENGTH_OF_SEQUENCE_MUST_POINT_TO_A_SEQUENCE_RULE);
    }
    if (memberSymbol.GetAttributes<WLengthOfStringAttribute>()
                    .Any(a => !a.OtherMember.IsString)) {
      memberSymbol.Report(WLENGTH_OF_STRING_MUST_POINT_TO_A_STRING_RULE);
    }
  }
}