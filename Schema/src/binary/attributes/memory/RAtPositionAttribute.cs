using System;


namespace schema.binary.attributes {
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class RAtPositionAttribute : Attribute {
    public RAtPositionAttribute(string offsetName) {
      this.OffsetName = offsetName;
    }

    public string OffsetName { get; }
  }
}
