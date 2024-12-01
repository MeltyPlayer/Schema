using System;


namespace schema.binary.attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class FixedPointAttribute(
    int signBits,
    int integerBits,
    int fractionBits) : BMemberAttribute {
  public int SignBits { get; set; } = signBits;
  public int IntegerBits { get; set; } = integerBits;
  public int FractionBits { get; set; } = fractionBits;

  protected override void InitFields() {
    this.memberThisIsAttachedTo_.AssertIsFloat();
  }
}