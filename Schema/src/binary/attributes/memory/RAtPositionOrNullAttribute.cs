using System;


namespace schema.binary.attributes {
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class RAtPositionOrNullAttribute(string offsetName, int nullValue = 0)
      : Attribute, IAtPositionAttribute {
    public string OffsetName { get; } = offsetName;
    public int? NullValue { get; } = nullValue;
  }
}