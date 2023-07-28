namespace schema.binary.attributes {
  public interface IStringLengthSourceAttribute {
    StringLengthSourceType Method { get; }

    SchemaIntegerType LengthType { get; }
    IMemberReference? OtherMember { get; }
    int ConstLength { get; }
  }
}