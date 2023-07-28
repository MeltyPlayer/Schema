using System;


namespace schema.binary.attributes {
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class WSizeOfStreamInBytesAttribute : Attribute { }
}