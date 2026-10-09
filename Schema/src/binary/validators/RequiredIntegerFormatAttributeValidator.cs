using Microsoft.CodeAnalysis.CSharp.Syntax;

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
  
  public static readonly Rule ENUM_NEEDS_INTEGER_FORMAT_RULE
      = new(
          $"Enum needs {nameof(IntegerFormatAttribute)}",
          $"Enum member '{{0}}' needs either a valid {nameof(IntegerFormatAttribute)} or for its enum type to specify an underlying representation.");

  public void Validate(IBetterSymbol memberSymbol, ITypeInfo typeInfo) {
    if (memberSymbol.HasAttribute<SkipAttribute>()) {
      return;
    }

    if (typeInfo.Kind == SchemaTypeKind.BOOL) {
      if (!memberSymbol.HasAttribute<IntegerFormatAttribute>()) {
        memberSymbol.Report(BOOLEAN_NEEDS_INTEGER_FORMAT_RULE);
      }
      return;
    }

    if (typeInfo.Kind == SchemaTypeKind.ENUM) {
      var syntaxReferences = typeInfo.TypeSymbol.DeclaringSyntaxReferences[0];
      var enumDeclarationSyntax = (syntaxReferences.GetSyntax() as EnumDeclarationSyntax)!;
      var hasManualType = enumDeclarationSyntax.BaseList is { Types.Count: > 0 };

      if (!hasManualType && !memberSymbol.HasAttribute<IntegerFormatAttribute>()) {
        memberSymbol.Report(ENUM_NEEDS_INTEGER_FORMAT_RULE);
      }
      return;
    }
  }
}