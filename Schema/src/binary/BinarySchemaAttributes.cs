using System;

using schema.binary.attributes;


namespace schema.binary {
  /// <summary>
  ///   Attribute for automatically generating Read/Write methods on
  ///   classes/structs. These are generated at compile-time, so the field
  ///   order will be 1:1 to the original class/struct and there should be no
  ///   performance cost compared to manually defined logic.
  ///
  ///   For any types that have this attribute, DO NOT modify or move around
  ///   the fields unless you know what you're doing!
  /// </summary>
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
  public class BinarySchemaAttribute : Attribute { }

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class NumberFormatAttribute : Attribute {
    public NumberFormatAttribute(SchemaNumberType numberType) {
      this.NumberType = numberType;
    }

    public SchemaNumberType NumberType { get; }
  }

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class IntegerFormatAttribute : Attribute {
    public IntegerFormatAttribute(SchemaIntegerType integerType) {
      this.IntegerType = integerType;
    }

    public SchemaIntegerType IntegerType { get; }
  }
}