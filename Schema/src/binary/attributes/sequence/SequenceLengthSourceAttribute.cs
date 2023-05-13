using System;

namespace schema.binary.attributes.sequence {
  public interface ISequenceLengthSourceAttribute {
    SequenceLengthSourceType Method { get; }

    SchemaIntegerType LengthType { get; }
    IMemberReference OtherMember { get; }
    uint ConstLength { get; }

  }

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

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class RSequenceLengthSourceAttribute : BMemberAttribute,
                                            ISequenceLengthSourceAttribute {
    private string? otherMemberName_;

    /// <summary>
    ///   Uses another integer field for the length. This separate field will
    ///   only be used when reading.
    /// </summary>
    public RSequenceLengthSourceAttribute(string otherMemberName) {
      this.Method = SequenceLengthSourceType.OTHER_MEMBER;
      this.otherMemberName_ = otherMemberName;
    }

    protected override void InitFields() {
      if (this.otherMemberName_ != null) {
        this.OtherMember =
            this.GetSourceRelativeToStructure(this.otherMemberName_)
                .AssertIsInteger();
      }
    }

    public SequenceLengthSourceType Method { get; }

    public SchemaIntegerType LengthType { get; }
    public IMemberReference OtherMember { get; private set; }
    public uint ConstLength { get; }
  }
}
