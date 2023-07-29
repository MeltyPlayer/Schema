using System;

namespace schema.binary.attributes {
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class SequenceLengthSourceAttribute : Attribute, ISequenceLengthSourceAttribute {
    /// <summary>
    ///   Parses an integer length with the given format immediately before the array.
    /// </summary>
    public SequenceLengthSourceAttribute(SchemaIntegerType lengthType) {
      this.Method = SequenceLengthSourceType.IMMEDIATE_VALUE;
      this.LengthType = lengthType;
    }

    /// <summary>
    ///   Uses a constant integer for the length.
    /// </summary>
    public SequenceLengthSourceAttribute(uint constLength) {
      this.Method = SequenceLengthSourceType.CONST_LENGTH;
      this.ConstLength = constLength;
    }

    public SequenceLengthSourceType Method { get; }

    public SchemaIntegerType LengthType { get; }
    public IMemberReference OtherMember { get; }
    public uint ConstLength { get; }
  }
}
