using schema.binary.parser;
using schema.util.symbols;

namespace schema.binary.validators;

public interface IValidator;

public interface IContainerValidator : IValidator {
  void Validate(IBinarySchemaContainerV2 container);
}

public interface IMemberValidator : IValidator {
  void Validate(IBetterSymbol memberSymbol, ITypeInfo typeInfo);
}