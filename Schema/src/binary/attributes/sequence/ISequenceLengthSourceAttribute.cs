namespace schema.binary.attributes;

public interface ISequenceLengthSourceAttribute {
  SequenceLengthSourceType Method { get; }

  SchemaIntegerType LengthType { get; }
  IMemberReference OtherMember { get; }
  uint ConstLength { get; }
}