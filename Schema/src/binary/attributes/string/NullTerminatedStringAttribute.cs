using System;


namespace schema.binary.attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class NullTerminatedStringAttribute : BMemberAttribute<string> {
  protected override void InitFields() { }
}