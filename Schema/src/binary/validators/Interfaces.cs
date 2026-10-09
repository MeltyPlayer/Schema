namespace schema.binary.validators;

internal interface IValidator;

internal interface IContainerValidator : IValidator {
  void Validate(IBinarySchemaContainerV2 container);
}

internal interface IMemberValidator : IValidator {
  void Validate(IBinarySchemaMemberV2 member);
}