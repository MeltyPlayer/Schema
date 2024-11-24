using System;


namespace schema.binary.attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class AlignEndAttribute : Attribute {
  public AlignEndAttribute(uint align) {
    this.Method = AlignSourceType.CONST;
    this.ConstAlign = align;
  }

  public AlignSourceType Method { get; }
  public uint ConstAlign { get; }
}