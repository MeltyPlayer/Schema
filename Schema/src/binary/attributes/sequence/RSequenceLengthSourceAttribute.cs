using System;


namespace schema.binary.attributes {
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
            this.GetReadTimeOnlySourceRelativeToContainer(this.otherMemberName_)
                .AssertIsInteger();
      }
    }

    public SequenceLengthSourceType Method { get; }

    public SchemaIntegerType LengthType { get; }
    public IMemberReference OtherMember { get; private set; }
    public uint ConstLength { get; }
  }
}