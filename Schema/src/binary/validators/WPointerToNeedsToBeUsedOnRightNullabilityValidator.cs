using schema.binary.attributes;
using schema.binary.parser;

namespace schema.binary.validators;

public sealed class WPointerToNeedsToBeUsedOnRightNullabilityValidator
    : IValueMemberValidator {
  public static readonly Rule
      WPOINTER_TO_CAN_ONLY_BE_USED_ON_NON_NULLABLE_RULE
          = new(
              $"{nameof(WPointerToAttribute)} can only be used on non-nullables",
              $"Member '{{0}}' points to a nullable member, so {nameof(WPointerToOrNullAttribute)} should be used instead.");

  public static readonly Rule
      WPOINTER_TO_OR_NULL_CAN_ONLY_BE_USED_ON_NULLABLE_RULE
          = new(
              $"{nameof(WPointerToOrNullAttribute)} can only be used on nullables",
              $"Member '{{0}}' points to a non-nullable member, so {nameof(WPointerToAttribute)} should be used instead.");

  public void Validate(ISchemaValueMember valueMember) {
    var memberSymbol = valueMember.MemberSymbol;
    var typeInfo = valueMember.MemberType.TypeInfo;

    if (typeInfo.Kind != SchemaTypeKind.INTEGER) {
      return;
    }

    if (memberSymbol.TryGetAttribute(out WPointerToAttribute wPointerTo) &&
        wPointerTo.AccessChainToOtherMember.Target.MemberTypeInfo.IsNullable) {
      memberSymbol.Report(WPOINTER_TO_CAN_ONLY_BE_USED_ON_NON_NULLABLE_RULE);
    }

    if (memberSymbol.TryGetAttribute(
            out WPointerToOrNullAttribute wPointerToOrNull) &&
        !wPointerToOrNull
            .AccessChainToOtherMember.Target.MemberTypeInfo
            .IsNullable) {
      memberSymbol.Report(WPOINTER_TO_OR_NULL_CAN_ONLY_BE_USED_ON_NULLABLE_RULE);
    }
  }
}