using System;


namespace schema.binary.attributes {
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class NumberFormatAttribute : Attribute {
    public NumberFormatAttribute(SchemaNumberType numberType) {
      this.NumberType = numberType;
    }

    public SchemaNumberType NumberType { get; }
  }
}