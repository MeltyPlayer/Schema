using schema.binary.parser;
using schema.util.symbols;

namespace schema.binary.validators;

internal interface IValidator;

internal interface IContainerValidator : IValidator {
  void Validate(IBinarySchemaContainerV2 container);
}

internal interface IMemberValidator : IValidator {
  void Validate(IBetterSymbol memberSymbol, ITypeInfo typeInfo);
}