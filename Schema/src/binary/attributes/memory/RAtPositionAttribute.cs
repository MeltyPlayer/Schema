using System;


namespace schema.binary.attributes {
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class RAtPositionAttribute : Attribute, IAtPositionAttribute {
    public RAtPositionAttribute(string offsetName) {
      this.OffsetName = offsetName;
    }

    public string OffsetName { get; }
    public int? NullValue => null;
  }
}