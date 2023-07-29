using System;
using Microsoft.CodeAnalysis;


namespace schema.binary {
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
  }
}