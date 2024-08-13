using System;


namespace schema.binary.attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class IntegerFormatAttribute : Attribute {
  public IntegerFormatAttribute(SchemaIntegerType integerType) {
      this.IntegerType = integerType;
    }

  public SchemaIntegerType IntegerType { get; }
}