using System;


namespace schema.binary.text;

public static class SchemaGeneratorUtil {
  public static bool TryToGetLabelForMethodWithoutCast(IMemberType memberType,
    out string label) {
    if (memberType is IPrimitiveMemberType { UseAltFormat: true }) {
      label = "";
      return false;
    }

    try {
      label = GetLabelForMethod(memberType);
      return true;
    } catch { }

    label = "";
    return false;
  }

  public static string GetLabelForMethod(IMemberType memberType)
    => memberType switch {
        IPrimitiveMemberType primitiveMemberType
            => SchemaGeneratorUtil
                .GetPrimitiveLabel(
                    primitiveMemberType.UseAltFormat
                        ? primitiveMemberType.AltFormat.AsPrimitiveType()
                        : primitiveMemberType.PrimitiveType),
        IKnownStructMemberType knownStructMemberType
            => SchemaGeneratorUtil.GetKnownStructName(
                knownStructMemberType.KnownStruct),
    };

  public static string GetPrimitiveLabel(SchemaPrimitiveType type)
    => type switch {
        SchemaPrimitiveType.CHAR => "Char",
        SchemaPrimitiveType.SBYTE => "SByte",
        SchemaPrimitiveType.BYTE => "Byte",
        SchemaPrimitiveType.INT16 => "Int16",
        SchemaPrimitiveType.UINT16 => "UInt16",
        SchemaPrimitiveType.INT24 => "Int24",
        SchemaPrimitiveType.UINT24 => "UInt24",
        SchemaPrimitiveType.INT32 => "Int32",
        SchemaPrimitiveType.UINT32 => "UInt32",
        SchemaPrimitiveType.INT64 => "Int64",
        SchemaPrimitiveType.UINT64 => "UInt64",
        SchemaPrimitiveType.HALF => "Half",
        SchemaPrimitiveType.SINGLE => "Single",
        SchemaPrimitiveType.DOUBLE => "Double",
        SchemaPrimitiveType.SN8 => "Sn8",
        SchemaPrimitiveType.UN8 => "Un8",
        SchemaPrimitiveType.SN16 => "Sn16",
        SchemaPrimitiveType.UN16 => "Un16",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

  public static string GetIntLabel(SchemaIntegerType type)
    => type switch {
        SchemaIntegerType.SBYTE => "SByte",
        SchemaIntegerType.BYTE => "Byte",
        SchemaIntegerType.INT16 => "Int16",
        SchemaIntegerType.UINT16 => "UInt16",
        SchemaIntegerType.INT24 => "Int24",
        SchemaIntegerType.UINT24 => "UInt24",
        SchemaIntegerType.INT32 => "Int32",
        SchemaIntegerType.UINT32 => "UInt32",
        SchemaIntegerType.INT64 => "Int64",
        SchemaIntegerType.UINT64 => "UInt64",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

  public static string GetTypeName(SchemaNumberType type)
    => type switch {
        SchemaNumberType.SBYTE => "sbyte",
        SchemaNumberType.BYTE => "byte",
        SchemaNumberType.INT16 => "short",
        SchemaNumberType.UINT16 => "ushort",
        SchemaNumberType.INT32 => "int",
        SchemaNumberType.UINT32 => "uint",
        SchemaNumberType.INT64 => "long",
        SchemaNumberType.UINT64 => "ulong",
        SchemaNumberType.HALF => "float",
        SchemaNumberType.SINGLE => "float",
        SchemaNumberType.DOUBLE => "double",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

  public static string GetEndiannessName(Endianness type)
    => type switch {
        Endianness.BigEndian => "Endianness.BigEndian",
        Endianness.LittleEndian => "Endianness.LittleEndian",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

  public static string GetKnownStructName(KnownStruct knownStruct)
    => knownStruct switch {
        KnownStruct.VECTOR2    => "Vector2",
        KnownStruct.VECTOR3    => "Vector3",
        KnownStruct.VECTOR4    => "Vector4",
        KnownStruct.MATRIX4X4  => "Matrix4x4",
        KnownStruct.MATRIX3X2  => "Matrix3x2",
        KnownStruct.QUATERNION => "Quaternion",
        _ => throw new ArgumentOutOfRangeException(
            nameof(knownStruct),
            knownStruct,
            null)
    };

  public static bool TryToGetSequenceAsSpan(
      ISequenceMemberType sequenceMemberType,
      ISchemaValueMember member,
      out string text) {
    switch (sequenceMemberType.SequenceTypeInfo.SequenceType) {
      case SequenceType.MUTABLE_ARRAY or SequenceType.IMMUTABLE_ARRAY: {
        text = $"this.{member.Name}";
        return true;
      }
      case SequenceType.MUTABLE_LIST: {
        text = $"this.{member.Name}.AsSpan()";
        return true;
      }
      default: {
        text = "";
        return false;
      }
    }
  }
}