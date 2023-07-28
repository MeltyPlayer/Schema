using System;


namespace schema.binary.attributes {
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class RAtOffsetAttribute : Attribute {
    public RAtOffsetAttribute(string offsetName) {
      this.OffsetName = offsetName;
    }

    public string OffsetName { get; }
  }
}
