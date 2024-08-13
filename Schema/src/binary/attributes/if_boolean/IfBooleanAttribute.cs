using System;


namespace schema.binary.attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class IfBooleanAttribute : Attribute, IIfBooleanAttribute {
  public IfBooleanAttribute(SchemaIntegerType lengthType) {
      this.SourceType = IfBooleanSourceType.IMMEDIATE_VALUE;
      this.ImmediateBooleanType = lengthType;
    }

  public IfBooleanSourceType SourceType { get; }

  public SchemaIntegerType ImmediateBooleanType { get; }
  public IMemberReference? OtherMember { get; }
}