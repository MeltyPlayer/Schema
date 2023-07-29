using System;

namespace schema.binary {
  public enum SchemaNumberType {
    UNDEFINED,

    SBYTE,
    BYTE,
    INT16,
    UINT16,
    INT24,
    UINT24,
    INT32,
    UINT32,
    INT64,
    UINT64,

    HALF,
    SINGLE,
    DOUBLE,

    SN8,
    UN8,

    SN16,
    UN16,
  }

  public static class SchemaNumberTypeExtensions {
    public static SchemaIntegerType AsIntegerType(this SchemaNumberType type)
      => type switch {
          SchemaNumberType.SBYTE => SchemaIntegerType.SBYTE,
          SchemaNumberType.BYTE => SchemaIntegerType.BYTE,
          SchemaNumberType.INT16 => SchemaIntegerType.INT16,
          SchemaNumberType.UINT16 => SchemaIntegerType.UINT16,
          SchemaNumberType.INT24 => SchemaIntegerType.INT24,
          SchemaNumberType.UINT24 => SchemaIntegerType.UINT24,
          SchemaNumberType.INT32 => SchemaIntegerType.INT32,
          SchemaNumberType.UINT32 => SchemaIntegerType.UINT32,
          SchemaNumberType.INT64 => SchemaIntegerType.INT64,
          SchemaNumberType.UINT64 => SchemaIntegerType.UINT64,
          _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
      };

    public static SchemaPrimitiveType AsPrimitiveType(
        this SchemaNumberType type)
      => type switch {
          SchemaNumberType.SBYTE => SchemaPrimitiveType.SBYTE,
          SchemaNumberType.BYTE => SchemaPrimitiveType.BYTE,
          SchemaNumberType.INT16 => SchemaPrimitiveType.INT16,
          SchemaNumberType.UINT16 => SchemaPrimitiveType.UINT16,
          SchemaNumberType.INT24 => SchemaPrimitiveType.INT24,
          SchemaNumberType.UINT24 => SchemaPrimitiveType.UINT24,
          SchemaNumberType.INT32 => SchemaPrimitiveType.INT32,
          SchemaNumberType.UINT32 => SchemaPrimitiveType.UINT32,
          SchemaNumberType.INT64 => SchemaPrimitiveType.INT64,
          SchemaNumberType.UINT64 => SchemaPrimitiveType.UINT64,
          SchemaNumberType.HALF => SchemaPrimitiveType.HALF,
          SchemaNumberType.SINGLE => SchemaPrimitiveType.SINGLE,
          SchemaNumberType.DOUBLE => SchemaPrimitiveType.DOUBLE,
          SchemaNumberType.SN8 => SchemaPrimitiveType.SN8,
          SchemaNumberType.UN8 => SchemaPrimitiveType.UN8,
          SchemaNumberType.SN16 => SchemaPrimitiveType.SN16,
          SchemaNumberType.UN16 => SchemaPrimitiveType.UN16,
          _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
      };
  }
}