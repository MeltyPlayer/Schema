using System;


namespace schema.binary.attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class AlignEndAttribute(uint align) : Attribute {
  public AlignSourceType Method => AlignSourceType.CONST;
  public uint ConstAlign { get; } = align;
}