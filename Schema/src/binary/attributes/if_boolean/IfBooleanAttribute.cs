using System;

namespace schema.binary.attributes {
  public enum IfBooleanSourceType {
    UNSPECIFIED,
    IMMEDIATE_VALUE,
    OTHER_MEMBER,
  }

  public interface IIfBooleanAttribute {
    IfBooleanSourceType SourceType { get; }

    SchemaIntegerType ImmediateBooleanType { get; }
    IMemberReference? OtherMember { get; }
  }

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class IfBooleanAttribute : Attribute, IIfBooleanAttribute {
    public IfBooleanAttribute(SchemaIntegerType lengthType) {
      this.SourceType = IfBooleanSourceType.IMMEDIATE_VALUE;
      this.ImmediateBooleanType = lengthType;
    }

    public IfBooleanSourceType SourceType { get; }

    public SchemaIntegerType ImmediateBooleanType { get; }
    public IMemberReference? OtherMember { get; }
  }

  public class RIfBooleanAttribute : BMemberAttribute, IIfBooleanAttribute {
    private readonly string? otherMemberName_;

    public RIfBooleanAttribute(string otherMemberName) {
      this.SourceType = IfBooleanSourceType.OTHER_MEMBER;
      this.otherMemberName_ = otherMemberName;
    }

    protected override void InitFields() {
      if (this.otherMemberName_ != null) {
        this.OtherMember =
            this.GetReadTimeOnlySourceRelativeToStructure(this.otherMemberName_)
                .AssertIsBool();
      }
    }

    public IfBooleanSourceType SourceType { get; }

    public SchemaIntegerType ImmediateBooleanType { get; }
    public IMemberReference? OtherMember { get; private set; }
  }
}