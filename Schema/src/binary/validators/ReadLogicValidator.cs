using Microsoft.CodeAnalysis;

using schema.binary.attributes;
using schema.util.symbols;

namespace schema.binary.validators;

public sealed class ReadLogicValidator : IMethodMemberValidator {
  public static readonly Rule
      READ_LOGIC_MUST_BE_USED_ON_A_METHOD_WITH_CORRECT_ARG
          = new(
              $"{nameof(ReadLogicAttribute)} must be used on a method with a single {nameof(IBinaryReader)} parameter",
              $"Member '{{0}}' must be a method with just a single {nameof(IBinaryReader)} parameter.");

  public void Validate(ISchemaMethodMember methodMember) {
    var memberSymbol = methodMember.MemberSymbol;
    var methodSymbol = memberSymbol.Symbol as IMethodSymbol;
    var isMethod = methodSymbol != null;
    var hasCorrectParameters
        = methodSymbol is { Parameters.Length: 1 } &&
          methodSymbol.Parameters[0]
                      .Type.IsType<IBinaryReader>();

    if (!(isMethod && hasCorrectParameters) &&
        memberSymbol.HasAttribute<ReadLogicAttribute>()) {
      memberSymbol.Report(
          READ_LOGIC_MUST_BE_USED_ON_A_METHOD_WITH_CORRECT_ARG);
    }
  }
}