using schema.binary.attributes;
using schema.binary.parser;
using schema.util.symbols;

namespace schema.binary.validators;

public sealed class WLengthOfAndPointerToAndSizeOfCanOnlyBeUsedOnIntegersValidator : IValueMemberValidator {
  public static readonly Rule
      WLENGTH_OF_SEQUENCE_CAN_ONLY_BE_USED_ON_INTEGERS_RULE
          = new(
              $"{nameof(WLengthOfSequenceAttribute)} can only be used on integers",
              $"Member '{{0}}' is not an integer, so it cannot be used with {nameof(WLengthOfSequenceAttribute)}.");
  
  public static readonly Rule
      WLENGTH_OF_STRING_CAN_ONLY_BE_USED_ON_INTEGERS_RULE
          = new(
              $"{nameof(WLengthOfStringAttribute)} can only be used on integers",
              $"Member '{{0}}' is not an integer, so it cannot be used with {nameof(WLengthOfStringAttribute)}.");

  public static readonly Rule
      WPOINTER_TO_CAN_ONLY_BE_USED_ON_INTEGERS_RULE
          = new(
              $"{nameof(WPointerToAttribute)} can only be used on integers",
              $"Member '{{0}}' is not an integer, so it cannot be used with {nameof(WPointerToAttribute)}.");

  public static readonly Rule
      WSIZE_OF_MEMBER_CAN_ONLY_BE_USED_ON_INTEGERS_RULE
          = new(
              $"{nameof(WSizeOfMemberInBytesAttribute)} can only be used on integers",
              $"Member '{{0}}' is not an integer, so it cannot be used with {nameof(WSizeOfMemberInBytesAttribute)}.");
  
  public static readonly Rule
      WSIZE_OF_STREAM_CAN_ONLY_BE_USED_ON_INTEGERS_RULE
          = new(
              $"{nameof(WSizeOfStreamInBytesAttribute)} can only be used on integers",
              $"Member '{{0}}' is not an integer, so it cannot be used with {nameof(WSizeOfStreamInBytesAttribute)}.");

  public void Validate(ISchemaValueMember valueMember) {
    var memberSymbol = valueMember.MemberSymbol;
    var typeInfo = valueMember.MemberType.TypeInfo;

    if (typeInfo.Kind == SchemaTypeKind.INTEGER) {
      return;
    }

    if (memberSymbol.HasAttribute<WLengthOfSequenceAttribute>()) {
      memberSymbol.Report(WLENGTH_OF_SEQUENCE_CAN_ONLY_BE_USED_ON_INTEGERS_RULE);
    }
    if (memberSymbol.HasAttribute<WLengthOfStringAttribute>()) {
      memberSymbol.Report(WLENGTH_OF_STRING_CAN_ONLY_BE_USED_ON_INTEGERS_RULE);
    }
    if (memberSymbol.HasAttribute<WPointerToAttribute>()) {
      memberSymbol.Report(WPOINTER_TO_CAN_ONLY_BE_USED_ON_INTEGERS_RULE);
    }
    if (memberSymbol.HasAttribute<WSizeOfMemberInBytesAttribute>()) {
      memberSymbol.Report(WSIZE_OF_MEMBER_CAN_ONLY_BE_USED_ON_INTEGERS_RULE);
    }
    if (memberSymbol.HasAttribute<WSizeOfStreamInBytesAttribute>()) {
      memberSymbol.Report(WSIZE_OF_STREAM_CAN_ONLY_BE_USED_ON_INTEGERS_RULE);
    }
  }
}