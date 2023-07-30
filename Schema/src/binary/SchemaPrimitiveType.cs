﻿using System;

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
          _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
      };

    public static SchemaIntegerType AsIntegerType(this SchemaPrimitiveType type)
      => type.AsNumberType().AsIntegerType();
  }
}