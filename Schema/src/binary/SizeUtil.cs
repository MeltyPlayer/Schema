﻿using System;

namespace schema.binary;

public static class SizeUtil {
  public static bool TryGetSizeOfType(IMemberType type, out int size) {
    switch (type) {
      case IPrimitiveMemberType primitiveMemberType: {
        if (primitiveMemberType.UseAltFormat) {
          size = GetSize(primitiveMemberType.AltFormat);
          return true;
        }

        return TryGetSize(primitiveMemberType.PrimitiveType, out size);
      }
      case IKnownStructMemberType knownStructMemberType: {
        size = GetSize(knownStructMemberType.KnownStruct);
        return true;
      }
      default: {
        size = -1;
        return false;
      }
    }
  }

  public static int GetSize(SchemaIntegerType type) => type switch {
      SchemaIntegerType.BYTE   => 1,
      SchemaIntegerType.SBYTE  => 1,
      SchemaIntegerType.INT16  => 2,
      SchemaIntegerType.UINT16 => 2,
      SchemaIntegerType.INT24  => 3,
      SchemaIntegerType.UINT24 => 3,
      SchemaIntegerType.INT32  => 4,
      SchemaIntegerType.UINT32 => 4,
      SchemaIntegerType.INT64  => 8,
      SchemaIntegerType.UINT64 => 8,
  };

  public static int GetSize(SchemaNumberType type) => type switch {
      SchemaNumberType.SBYTE  => 1,
      SchemaNumberType.BYTE   => 1,
      SchemaNumberType.INT16  => 2,
      SchemaNumberType.UINT16 => 2,
      SchemaNumberType.INT24  => 3,
      SchemaNumberType.UINT24 => 3,
      SchemaNumberType.INT32  => 4,
      SchemaNumberType.UINT32 => 4,
      SchemaNumberType.INT64  => 8,
      SchemaNumberType.UINT64 => 8,
      SchemaNumberType.HALF   => 2,
      SchemaNumberType.SINGLE => 4,
      SchemaNumberType.DOUBLE => 8,
      SchemaNumberType.SN8    => 1,
      SchemaNumberType.UN8    => 1,
      SchemaNumberType.SN16   => 2,
      SchemaNumberType.UN16   => 2,
  };

  public static int GetSize(KnownStruct type) => type switch {
      KnownStruct.VECTOR2 => 2 * 4,
      KnownStruct.VECTOR3 => 3 * 4,
      KnownStruct.VECTOR4 => 4 * 4,
      KnownStruct.MATRIX4X4 => 4 * 4 * 4,
      KnownStruct.MATRIX3X2 => 3 * 2 * 4,
      KnownStruct.QUATERNION => 4 * 4,
      _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
  };

  public static bool TryGetSize(SchemaPrimitiveType primitiveType,
                                out int size) {
    var numberType = primitiveType.AsNumberType();
    if (numberType == SchemaNumberType.UNDEFINED) {
      size = -1;
      return false;
    }

    size = GetSize(numberType);
    return true;
  }
}