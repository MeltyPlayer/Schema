using System;

namespace schema.binary.attributes {
  public interface IIfBooleanAttribute {
    IfBooleanSourceType Method { get; }

    SchemaIntegerType BooleanType { get; }
    IMemberReference? OtherMember { get; }
  }

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class IfBooleanAttribute : Attribute, IIfBooleanAttribute {
    public IfBooleanAttribute(SchemaIntegerType lengthType) {
      this.Method = IfBooleanSourceType.IMMEDIATE_VALUE;
      this.BooleanType = lengthType;
    }

    public IfBooleanSourceType Method { get; }

    public SchemaIntegerType BooleanType { get; }
    public IMemberReference? OtherMember { get; }
  }

  public class RIfBooleanAttribute : BMemberAttribute, IIfBooleanAttribute {
    private readonly string? otherMemberName_;

    public RIfBooleanAttribute(string otherMemberName) {
      this.Method = IfBooleanSourceType.OTHER_MEMBER;
      this.otherMemberName_ = otherMemberName;
    }

    protected override void InitFields() {
      if (this.otherMemberName_ != null) {
        this.OtherMember =
            this.GetReadTimeOnlySourceRelativeToStructure(this.otherMemberName_)
                .AssertIsBool();
      }
    }

    public IfBooleanSourceType Method { get; }

    public SchemaIntegerType BooleanType { get; }
    public IMemberReference? OtherMember { get; private set; }
  }
}
