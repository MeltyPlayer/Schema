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
  public class NullTerminatedStringAttribute : BMemberAttribute<string> {
    protected override void InitFields() { }
  }

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


  public interface IIfBooleanAttribute {
    IfBooleanSourceType Method { get; }

    SchemaIntegerType BooleanType { get; }
    IMemberReference? OtherMember { get; }
  }

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class IfBooleanAttribute : Attribute, IIfBooleanAttribute {
    public IfBooleanAttribute(SchemaIntegerType lengthType) {
      this.Method = IfBooleanSourceType.IMMEDIATE_VALUE;
      this.BooleanType = lengthType;
    }

    public IfBooleanSourceType Method { get; }

    public SchemaIntegerType BooleanType { get; }
    public IMemberReference? OtherMember { get; }
  }

  public class RIfBooleanAttribute : BMemberAttribute, IIfBooleanAttribute {
    private readonly string? otherMemberName_;

    public RIfBooleanAttribute(string otherMemberName) {
      this.Method = IfBooleanSourceType.OTHER_MEMBER;
      this.otherMemberName_ = otherMemberName;
    }

    protected override void InitFields() {
      if (this.otherMemberName_ != null) {
        this.OtherMember =
            this.GetSourceRelativeToStructure(this.otherMemberName_)
                .AssertIsBool();
      }
    }

    public IfBooleanSourceType Method { get; }

    public SchemaIntegerType BooleanType { get; }
    public IMemberReference? OtherMember { get; private set; }
  }
}