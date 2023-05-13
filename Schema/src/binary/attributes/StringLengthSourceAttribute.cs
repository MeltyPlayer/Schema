using System;

using schema.binary.attributes;


namespace schema.binary {
  public interface IStringLengthSourceAttribute {
    StringLengthSourceType Method { get; }

    SchemaIntegerType LengthType { get; }
    IMemberReference? OtherMember { get; }
    int ConstLength { get; }
  }

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class StringLengthSourceAttribute : Attribute,
                                             IStringLengthSourceAttribute {
    /// <summary>
    ///   Parses a length with the given format immediately before the string.
    /// </summary>
    public StringLengthSourceAttribute(SchemaIntegerType lengthType) {
      this.Method = StringLengthSourceType.IMMEDIATE_VALUE;
      this.LengthType = lengthType;
    }

    public StringLengthSourceAttribute(int constLength) {
      this.Method = StringLengthSourceType.CONST;
      this.ConstLength = constLength;
    }

    public StringLengthSourceType Method { get; }

    public SchemaIntegerType LengthType { get; }
    public IMemberReference? OtherMember { get; }
    public int ConstLength { get; }
  }

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class RStringLengthSourceAttribute : BMemberAttribute<string>,
                                              IStringLengthSourceAttribute {
    private string? otherMemberName_;

    /// <summary>
    ///   Uses another field for the length. This separate field will only be used when
    ///   reading/writing.
    /// </summary>
    public RStringLengthSourceAttribute(string otherMemberName) {
      this.Method = StringLengthSourceType.OTHER_MEMBER;
      this.otherMemberName_ = otherMemberName;
    }

    protected override void InitFields() {
      if (this.otherMemberName_ != null) {
        this.OtherMember =
            this.GetSourceRelativeToStructure(this.otherMemberName_)
                .AssertIsInteger();
      }
    }

    public StringLengthSourceType Method { get; }

    public SchemaIntegerType LengthType { get; }
    public IMemberReference? OtherMember { get; private set; }
    public int ConstLength { get; }
  }
}