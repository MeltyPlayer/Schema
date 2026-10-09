using Microsoft.CodeAnalysis;

using schema.binary.parser;
using schema.util.symbols;

namespace schema.binary.validators;

public interface IValidator;

public interface IContainerValidator : IValidator {
  void Validate(IBetterSymbol<INamedTypeSymbol> containerSymbol);
}

public interface IMemberValidator : IValidator {
  void Validate(IBetterSymbol memberSymbol, ITypeInfo typeInfo);
}