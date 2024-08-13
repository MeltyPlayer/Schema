namespace schema.binary.attributes;

public interface IIfBooleanAttribute {
  IfBooleanSourceType SourceType { get; }

  SchemaIntegerType ImmediateBooleanType { get; }
  IMemberReference? OtherMember { get; }
}