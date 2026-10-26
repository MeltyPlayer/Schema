using Microsoft.CodeAnalysis;

using schema.binary.attributes;
using schema.binary.parser;
using schema.util.symbols;

namespace schema.binary.validators;

public sealed class RSequenceUntilEndOfStreamCannotBeUsedOnTypeWithSize0Validator
    : IValueMemberValidator {
  public static readonly Rule RSEQUENCE_END_OF_STREAM_CANNOT_BE_USED_WITH_EMPTY_ELEMENTS
      = new(
          $"{nameof(RSequenceUntilEndOfStreamAttribute)} cannot be used with empty elements",
          $"Member '{{0}}' is an array whose element are size 0, so {nameof(RSequenceUntilEndOfStreamAttribute)} would run in an infinite loop.");

  public void Validate(ISchemaValueMember valueMember) {
    var memberSymbol = valueMember.MemberSymbol;
    var typeInfo = valueMember.MemberType.TypeInfo;

    if (memberSymbol.HasAttribute<RSequenceUntilEndOfStreamAttribute>() &&
        valueMember.MemberType is ISequenceMemberType { ElementType: var elementType } &&
        SizeUtil.TryGetSizeOfType(elementType, out var size) && 
        size == 0) {
      memberSymbol.Report(RSEQUENCE_END_OF_STREAM_CANNOT_BE_USED_WITH_EMPTY_ELEMENTS);
    }
  }
}