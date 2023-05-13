using System;
using System.IO;


namespace schema.binary.text {
  public static class SchemaGeneratorUtil {
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
  }
}