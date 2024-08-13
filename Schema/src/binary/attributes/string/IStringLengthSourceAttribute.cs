namespace schema.binary.attributes;

public interface IStringLengthSourceAttribute {
  StringLengthSourceType Method { get; }

  SchemaIntegerType ImmediateLengthType { get; }
  IMemberReference? OtherMember { get; }
  int ConstLength { get; }
}