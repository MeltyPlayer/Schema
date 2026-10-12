using schema.binary.attributes;
using schema.binary.parser;
using schema.util.symbols;

namespace schema.binary.validators;

public sealed class RequiredIntegerFormatAttributeValidator
    : IMemberValidator {
  public static readonly Rule BOOLEAN_NEEDS_INTEGER_FORMAT_RULE
      = new(
          $"Boolean needs {nameof(IntegerFormatAttribute)}",
          $"Boolean member '{{0}}' needs a valid {nameof(IntegerFormatAttribute)}.");

  public void Validate(IBetterSymbol memberSymbol, ITypeInfo typeInfo) {
    if (typeInfo.Kind == SchemaTypeKind.BOOL) {
      if (!memberSymbol.HasAttribute<IntegerFormatAttribute>()) {
        memberSymbol.Report(BOOLEAN_NEEDS_INTEGER_FORMAT_RULE);
      }
      return;
    }
  }
}