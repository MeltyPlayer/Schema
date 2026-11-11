using schema.binary.attributes;
using schema.binary.parser;

namespace schema.binary.validators;

public sealed class WPointerToCanOnlyBeUsedOnIntegersValidator
    : IValueMemberValidator {
  public static readonly Rule WPOINTER_TO_CAN_ONLY_BE_USED_ON_INTEGERS_RULE
      = new(
          $"{nameof(WPointerToAttribute)} can only be used on integers",
          $"Member '{{0}}' is not an integer, so it cannot have a {nameof(WPointerToAttribute)}.");

  public static readonly Rule WPOINTER_TO_OR_NULL_CAN_ONLY_BE_USED_ON_INTEGERS_RULE
      = new(
          $"{nameof(WPointerToOrNullAttribute)} can only be used on sequences",
          $"Member '{{0}}' is not an integer, so it cannot have a {nameof(WPointerToOrNullAttribute)}.");

  public void Validate(ISchemaValueMember valueMember) {
    var memberSymbol = valueMember.MemberSymbol;
    var typeInfo = valueMember.MemberType.TypeInfo;

    if (typeInfo.Kind == SchemaTypeKind.INTEGER) {
      return;
    }

    if (memberSymbol.HasAttribute<WPointerToAttribute>()) {
      memberSymbol.Report(WPOINTER_TO_CAN_ONLY_BE_USED_ON_INTEGERS_RULE);
    }

    if (memberSymbol.HasAttribute<WPointerToOrNullAttribute>()) {
      memberSymbol.Report(WPOINTER_TO_OR_NULL_CAN_ONLY_BE_USED_ON_INTEGERS_RULE);
    }
  }
}