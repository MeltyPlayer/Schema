using System;
using Microsoft.CodeAnalysis;


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

  public static class SchemaPrimitiveTypesUtil {
    public static SchemaPrimitiveType GetPrimitiveTypeFromTypeSymbol(
        ITypeSymbol typeSymbol) {
      if (typeSymbol.TypeKind == TypeKind.Enum) {
        return SchemaPrimitiveType.ENUM;
      }

      return typeSymbol.SpecialType switch {
          SpecialType.System_Boolean => SchemaPrimitiveType.BOOLEAN,
          SpecialType.System_Char    => SchemaPrimitiveType.CHAR,
          SpecialType.System_SByte   => SchemaPrimitiveType.SBYTE,
          SpecialType.System_Byte    => SchemaPrimitiveType.BYTE,
          SpecialType.System_Int16   => SchemaPrimitiveType.INT16,
          SpecialType.System_UInt16  => SchemaPrimitiveType.UINT16,
          SpecialType.System_Int32   => SchemaPrimitiveType.INT32,
          SpecialType.System_UInt32  => SchemaPrimitiveType.UINT32,
          SpecialType.System_Int64   => SchemaPrimitiveType.INT64,
          SpecialType.System_UInt64  => SchemaPrimitiveType.UINT64,
          SpecialType.System_Single  => SchemaPrimitiveType.SINGLE,
          SpecialType.System_Double  => SchemaPrimitiveType.DOUBLE,
          _                          => SchemaPrimitiveType.UNDEFINED
      };
    }

    public static bool CanPrimitiveTypeBeReadAsNumber(SchemaPrimitiveType type)
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

    public static bool CanPrimitiveTypeBeReadAsInteger(SchemaPrimitiveType type)
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

    public static SchemaIntegerType GetIntegerTypeFromTypeSymbol(
        ITypeSymbol? typeSymbol)
      => typeSymbol?.SpecialType switch {
          SpecialType.System_Byte   => SchemaIntegerType.BYTE,
          SpecialType.System_SByte  => SchemaIntegerType.SBYTE,
          SpecialType.System_Int16  => SchemaIntegerType.INT16,
          SpecialType.System_UInt16 => SchemaIntegerType.UINT16,
          SpecialType.System_Int32  => SchemaIntegerType.INT32,
          SpecialType.System_UInt32 => SchemaIntegerType.UINT32,
          SpecialType.System_Int64  => SchemaIntegerType.INT64,
          SpecialType.System_UInt64 => SchemaIntegerType.UINT64,
          _                         => SchemaIntegerType.UNDEFINED,
      };

    public static SchemaNumberType ConvertIntToNumber(SchemaIntegerType type)
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

    public static SchemaPrimitiveType GetUnderlyingPrimitiveType(
        SchemaPrimitiveType type)
      => type switch {
          SchemaPrimitiveType.INT24  => SchemaPrimitiveType.INT32,
          SchemaPrimitiveType.UINT24 => SchemaPrimitiveType.UINT32,
          SchemaPrimitiveType.SN8    => SchemaPrimitiveType.SINGLE,
          SchemaPrimitiveType.UN8    => SchemaPrimitiveType.SINGLE,
          SchemaPrimitiveType.UN16   => SchemaPrimitiveType.SINGLE,
          SchemaPrimitiveType.SN16   => SchemaPrimitiveType.SINGLE,
          _                          => type
      };

    public static SchemaIntegerType ConvertNumberToInt(
        SchemaNumberType type)
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

    public static SchemaPrimitiveType ConvertNumberToPrimitive(
        SchemaNumberType type)
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

    public static SchemaNumberType ConvertPrimitiveToNumber(
        SchemaPrimitiveType type)
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
  }
}