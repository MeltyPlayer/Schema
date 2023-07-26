using System;

namespace schema.binary.attributes.align {
  public enum AlignSourceType {
    UNSPECIFIED,
    CONST,
    OTHER_MEMBER,
  }

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class AlignAttribute : BMemberAttribute {
    private string? otherMemberName_;

    public AlignAttribute(uint align) {
      this.Method = AlignSourceType.CONST;
      this.ConstAlign = align;
    }

    public AlignAttribute(string otherMemberName) {
      this.Method = AlignSourceType.OTHER_MEMBER;
      this.otherMemberName_ = otherMemberName;
    }

    protected override void InitFields() {
      if (this.otherMemberName_ != null) {
        this.OtherMember =
            this.GetOtherMemberRelativeToStructure(this.otherMemberName_)
                .AssertIsInteger();
      }
    }

    public AlignSourceType Method { get; }

    public IMemberReference OtherMember { get; private set; }
    public uint ConstAlign { get; }
  }
}
