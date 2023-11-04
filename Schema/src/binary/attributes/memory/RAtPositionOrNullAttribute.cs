using System;


namespace schema.binary.attributes {
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class RAtPositionOrNullAttribute(string offsetName, long nullValue = 0)
      : Attribute, IAtPositionAttribute {
    public string OffsetName { get; } = offsetName;
    public long? NullValue { get; } = nullValue;
  }
}