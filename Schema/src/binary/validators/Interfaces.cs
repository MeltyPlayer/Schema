using Microsoft.CodeAnalysis;

using schema.binary.parser;
using schema.util.symbols;

namespace schema.binary.validators;

public interface IValidator;

public interface IContainerValidator : IValidator {
  void Validate(IBetterSymbol<INamedTypeSymbol> containerSymbol);
}

public interface IMethodMemberValidator : IValidator {
  void Validate(ISchemaMethodMember methodMember);
}

public interface IValueMemberValidator : IValidator {
  void Validate(ISchemaValueMember valueMember);
}