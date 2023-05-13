using System;


namespace schema.binary.attributes.size {
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class WSizeOfStreamInBytesAttribute : Attribute { }
}