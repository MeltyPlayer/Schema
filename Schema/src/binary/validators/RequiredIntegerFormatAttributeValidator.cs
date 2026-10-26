using schema.binary.attributes;
using schema.binary.parser;

namespace schema.binary.validators;

public sealed class RequiredIntegerFormatAttributeValidator
    : IValueMemberValidator {
  public static readonly Rule BOOLEAN_NEEDS_INTEGER_FORMAT_RULE
      = new(
          $"Boolean needs {nameof(IntegerFormatAttribute)}",
          $"Boolean member '{{0}}' needs a valid {nameof(IntegerFormatAttribute)}.");

  public void Validate(ISchemaValueMember valueMember) {
    var memberSymbol = valueMember.MemberSymbol;
    var typeInfo = valueMember.MemberType.TypeInfo;
    if (typeInfo.Kind == SchemaTypeKind.BOOL) {
      if (!memberSymbol.HasAttribute<IntegerFormatAttribute>()) {
        memberSymbol.Report(BOOLEAN_NEEDS_INTEGER_FORMAT_RULE);
      }
    }
  }
}