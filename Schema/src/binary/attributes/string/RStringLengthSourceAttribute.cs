using System;


namespace schema.binary.attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class RStringLengthSourceAttribute
    : BMemberAttribute<string>,
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
          this.GetReadTimeOnlySourceRelativeToContainer(this.otherMemberName_)
              .AssertIsInteger();
    }
  }

  public StringLengthSourceType Method { get; }

  public SchemaIntegerType ImmediateLengthType { get; }
  public IMemberReference? OtherMember { get; private set; }
  public int ConstLength { get; }
}