using System;


namespace schema.binary.attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class StringLengthSourceAttribute
    : Attribute,
      IStringLengthSourceAttribute {
  /// <summary>
  ///   Parses a length with the given format immediately before the string.
  /// </summary>
  public StringLengthSourceAttribute(SchemaIntegerType lengthType) {
    this.Method = StringLengthSourceType.IMMEDIATE_VALUE;
    this.ImmediateLengthType = lengthType;
  }

  public StringLengthSourceAttribute(int constLength) {
    this.Method = StringLengthSourceType.CONST;
    this.ConstLength = constLength;
  }

  public StringLengthSourceType Method { get; }

  public SchemaIntegerType ImmediateLengthType { get; }
  public IMemberReference? OtherMember { get; }
  public int ConstLength { get; }
}