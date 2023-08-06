using System;

namespace schema.binary {
  public enum SchemaPrimitiveType {
    UNDEFINED,

    BOOLEAN,

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

    CHAR,

    ENUM,
  }

  public static class SchemaPrimitiveTypeExtensions {
    public static SchemaNumberType AsNumberType(this SchemaPrimitiveType type)
      => type switch {
          SchemaPrimitiveType.SBYTE => SchemaNumberType.SBYTE,
          SchemaPrimitiveType.BYTE => SchemaNumberType.BYTE,
          SchemaPrimitiveType.INT16 => SchemaNumberType.INT16,
          SchemaPrimitiveType.UINT16 => SchemaNumberType.UINT16,
          SchemaPrimitiveType.INT24 => SchemaNumberType.INT24,
          SchemaPrimitiveType.UINT24 => SchemaNumberType.UINT24,
          SchemaPrimitiveType.INT32 => SchemaNumberType.INT32,
          SchemaPrimitiveType.UINT32 => SchemaNumberType.UINT32,
          SchemaPrimitiveType.INT64 => SchemaNumberType.INT64,
          SchemaPrimitiveType.UINT64 => SchemaNumberType.UINT64,
          SchemaPrimitiveType.HALF => SchemaNumberType.HALF,
          SchemaPrimitiveType.SINGLE => SchemaNumberType.SINGLE,
          SchemaPrimitiveType.DOUBLE => SchemaNumberType.DOUBLE,
          SchemaPrimitiveType.SN8 => SchemaNumberType.SN8,
          SchemaPrimitiveType.UN8 => SchemaNumberType.UN8,
          SchemaPrimitiveType.SN16 => SchemaNumberType.SN16,
          SchemaPrimitiveType.UN16 => SchemaNumberType.UN16,
          SchemaPrimitiveType.UNDEFINED => SchemaNumberType.UNDEFINED,
          _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
      };

    public static SchemaIntegerType AsIntegerType(this SchemaPrimitiveType type)
      => type.AsNumberType().AsIntegerType();

    public static bool CanBeReadAsNumber(this SchemaPrimitiveType type)
      => type switch {
          SchemaPrimitiveType.BOOLEAN => false,
          SchemaPrimitiveType.SBYTE   => true,
          SchemaPrimitiveType.BYTE    => true,
          SchemaPrimitiveType.INT16   => true,
          SchemaPrimitiveType.UINT16  => true,
          SchemaPrimitiveType.INT24   => true,
          SchemaPrimitiveType.UINT24  => true,
          SchemaPrimitiveType.INT32   => true,
          SchemaPrimitiveType.UINT32  => true,
          SchemaPrimitiveType.INT64   => true,
          SchemaPrimitiveType.UINT64  => true,
          SchemaPrimitiveType.HALF    => true,
          SchemaPrimitiveType.SINGLE  => true,
          SchemaPrimitiveType.DOUBLE  => true,
          SchemaPrimitiveType.SN8     => true,
          SchemaPrimitiveType.UN8     => true,
          SchemaPrimitiveType.SN16    => true,
          SchemaPrimitiveType.UN16    => true,
          SchemaPrimitiveType.ENUM    => false,

          SchemaPrimitiveType.CHAR      => false,
          SchemaPrimitiveType.UNDEFINED => false,
          _                             => throw new NotImplementedException(),
      };

    public static bool CanBeReadAsInteger(this SchemaPrimitiveType type)
      => type switch {
          SchemaPrimitiveType.BOOLEAN => true,
          SchemaPrimitiveType.SBYTE   => true,
          SchemaPrimitiveType.BYTE    => true,
          SchemaPrimitiveType.INT16   => true,
          SchemaPrimitiveType.UINT16  => true,
          SchemaPrimitiveType.INT24   => true,
          SchemaPrimitiveType.UINT24  => true,
          SchemaPrimitiveType.INT32   => true,
          SchemaPrimitiveType.UINT32  => true,
          SchemaPrimitiveType.INT64   => true,
          SchemaPrimitiveType.UINT64  => true,
          SchemaPrimitiveType.HALF    => true,
          SchemaPrimitiveType.SINGLE  => true,
          SchemaPrimitiveType.DOUBLE  => true,
          SchemaPrimitiveType.SN8     => true,
          SchemaPrimitiveType.UN8     => true,
          SchemaPrimitiveType.SN16    => true,
          SchemaPrimitiveType.UN16    => true,
          SchemaPrimitiveType.ENUM    => true,

          SchemaPrimitiveType.CHAR      => false,
          SchemaPrimitiveType.UNDEFINED => false,
          _                             => throw new NotImplementedException(),
      };

    public static SchemaPrimitiveType GetUnderlyingPrimitiveType(
        this SchemaPrimitiveType type)
      => type switch {
          SchemaPrimitiveType.INT24  => SchemaPrimitiveType.INT32,
          SchemaPrimitiveType.UINT24 => SchemaPrimitiveType.UINT32,
          SchemaPrimitiveType.SN8    => SchemaPrimitiveType.SINGLE,
          SchemaPrimitiveType.UN8    => SchemaPrimitiveType.SINGLE,
          SchemaPrimitiveType.UN16   => SchemaPrimitiveType.SINGLE,
          SchemaPrimitiveType.SN16   => SchemaPrimitiveType.SINGLE,
          _                          => type
      };
  }
}