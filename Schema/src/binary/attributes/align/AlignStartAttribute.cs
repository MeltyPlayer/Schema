﻿using System;


namespace schema.binary.attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class AlignStartAttribute : BMemberAttribute {
  private string? otherMemberName_;

  public AlignStartAttribute(uint align) {
    this.Method = AlignSourceType.CONST;
    this.ConstAlign = align;
  }

  public AlignStartAttribute(string otherMemberName) {
    this.Method = AlignSourceType.OTHER_MEMBER;
    this.otherMemberName_ = otherMemberName;
  }

  protected override void InitFields() {
    if (this.otherMemberName_ != null) {
      this.OtherMember =
          this.GetOtherMemberRelativeToContainer(this.otherMemberName_)
              .AssertIsInteger();
    }
  }

  public AlignSourceType Method { get; }

  public IMemberReference OtherMember { get; private set; }
  public uint ConstAlign { get; }
}