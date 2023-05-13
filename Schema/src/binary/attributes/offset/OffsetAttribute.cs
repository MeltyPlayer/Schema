using System;


namespace schema.binary.attributes.offset {
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class OffsetAttribute : Attribute {
    public OffsetAttribute(string startIndexName, string offsetName) {
      this.StartIndexName = startIndexName;
      this.OffsetName = offsetName;
    }

    public string StartIndexName { get; }

    public string OffsetName { get; }
  }
}
