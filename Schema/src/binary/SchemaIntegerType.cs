using System;

namespace schema.binary {
  public enum SchemaIntegerType {
    UNDEFINED,

    BYTE,
    SBYTE,
    INT16,
    UINT16,
    INT24,
    UINT24,
    INT32,
    UINT32,
    INT64,
    UINT64
  }

  public static class SchemaIntegerTypeExtensions {
    public static SchemaNumberType AsNumberType(this SchemaIntegerType type)
      => type switch {
          SchemaIntegerType.SBYTE => SchemaNumberType.SBYTE,
          SchemaIntegerType.BYTE => SchemaNumberType.BYTE,
          SchemaIntegerType.INT16 => SchemaNumberType.INT16,
          SchemaIntegerType.UINT16 => SchemaNumberType.UINT16,
          SchemaIntegerType.INT24 => SchemaNumberType.INT24,
          SchemaIntegerType.UINT24 => SchemaNumberType.UINT24,
          SchemaIntegerType.INT32 => SchemaNumberType.INT32,
          SchemaIntegerType.UINT32 => SchemaNumberType.UINT32,
          SchemaIntegerType.INT64 => SchemaNumberType.INT64,
          SchemaIntegerType.UINT64 => SchemaNumberType.UINT64,
          _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
      };

    public static SchemaPrimitiveType AsPrimitiveType(
        this SchemaIntegerType type)
      => type.AsNumberType().AsPrimitiveType();

    public static bool CanAcceptAnInt32(this SchemaIntegerType type)
      => type switch {
          SchemaIntegerType.BYTE   => false,
          SchemaIntegerType.SBYTE  => false,
          SchemaIntegerType.INT16  => false,
          SchemaIntegerType.UINT16 => false,
          SchemaIntegerType.INT24  => true,
          SchemaIntegerType.UINT24 => false,
          SchemaIntegerType.INT32  => true,
          SchemaIntegerType.UINT32 => false,
          SchemaIntegerType.INT64  => true,
          SchemaIntegerType.UINT64 => false,
          _                        => false,
      };

    public static bool CanBeStoredInAnInt32(this SchemaIntegerType type)
      => type switch {
          SchemaIntegerType.BYTE   => true,
          SchemaIntegerType.SBYTE  => true,
          SchemaIntegerType.INT16  => true,
          SchemaIntegerType.UINT16 => true,
          SchemaIntegerType.INT24  => true,
          SchemaIntegerType.UINT24 => false,
          SchemaIntegerType.INT32  => true,
          SchemaIntegerType.UINT32 => false,
          SchemaIntegerType.INT64  => false,
          SchemaIntegerType.UINT64 => false,
          _                        => false,
      };
  }
}