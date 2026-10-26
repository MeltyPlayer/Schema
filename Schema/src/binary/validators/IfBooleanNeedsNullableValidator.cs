using schema.binary.attributes;
using schema.binary.parser;
using schema.util.symbols;

namespace schema.binary.validators;

public sealed class IfBooleanNeedsNullableValidator
    : IValueMemberValidator {
  public static readonly Rule IF_BOOLEAN_NEEDS_NULLABLE_TYPE
      = new(
          $"{nameof(IfBooleanAttribute)} needs nullable type",
          $"Member '{{0}}' must be a nullable type to use {nameof(IfBooleanAttribute)}.");

  public void Validate(ISchemaValueMember valueMember) {
    var memberSymbol = valueMember.MemberSymbol;
    var typeInfo = valueMember.MemberType.TypeInfo;

    if (!typeInfo.IsNullable &&
        (memberSymbol.HasAttribute<IfBooleanAttribute>() ||
         memberSymbol.HasAttribute<RIfBooleanAttribute>())) {
      memberSymbol.Report(IF_BOOLEAN_NEEDS_NULLABLE_TYPE);
    }
  }
}