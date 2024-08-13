namespace schema.binary.attributes;

public class RIfBooleanAttribute : BMemberAttribute, IIfBooleanAttribute {
  private readonly string? otherMemberName_;

  public RIfBooleanAttribute(string otherMemberName) {
      this.SourceType = IfBooleanSourceType.OTHER_MEMBER;
      this.otherMemberName_ = otherMemberName;
    }

  protected override void InitFields() {
      if (this.otherMemberName_ != null) {
        this.OtherMember =
            this.GetReadTimeOnlySourceRelativeToContainer(this.otherMemberName_)
                .AssertIsBool();
      }
    }

  public IfBooleanSourceType SourceType { get; }

  public SchemaIntegerType ImmediateBooleanType { get; }
  public IMemberReference? OtherMember { get; private set; }
}