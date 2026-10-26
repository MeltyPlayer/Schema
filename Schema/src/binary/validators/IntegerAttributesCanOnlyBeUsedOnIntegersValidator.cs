using schema.binary.attributes;
using schema.binary.parser;
using schema.util.symbols;

namespace schema.binary.validators;

public sealed class IntegerAttributesCanOnlyBeUsedOnIntegersValidator
    : IValueMemberValidator {
  public static readonly Rule INTEGER_FORMAT_CAN_ONLY_BE_USED_ON_INTEGERS_RULE
      = new(
          $"{nameof(IntegerFormatAttribute)} can only be used on integers",
          $"Member '{{0}}' is not an integer, so it cannot have a {nameof(IntegerFormatAttribute)}.");

  public void Validate(ISchemaValueMember valueMember) {
    var memberSymbol = valueMember.MemberSymbol;
    var typeInfo = valueMember.MemberType.TypeInfo;

    if (typeInfo.Kind is SchemaTypeKind.BOOL
                         or SchemaTypeKind.INTEGER
                         or SchemaTypeKind.ENUM ||
        typeInfo is ISequenceTypeInfo {
            ElementTypeInfo.Kind: SchemaTypeKind.BOOL
                                  or SchemaTypeKind.INTEGER
                                  or SchemaTypeKind.ENUM
        }) {
      return;
    }

    if (memberSymbol.HasAttribute<IntegerFormatAttribute>()) {
      memberSymbol.Report(INTEGER_FORMAT_CAN_ONLY_BE_USED_ON_INTEGERS_RULE);
    }
  }
}