using System;

namespace schema.defaultinterface {
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
  public class IncludeDefaultInterfaceMethodsAttribute : Attribute { }
}