using System;
using System.Reflection;

using Microsoft.CodeAnalysis;

using schema.binary.attributes;
using schema.binary.parser;
using schema.binary.steps;
using schema.binary.text;
using schema.util.symbols;

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
      case IStringType stringType: {
        if (stringType.LengthSourceType is StringLengthSourceType.CONST &&
            TryGetEncodingCharLength(
                stringType.EncodingType,
                out var charSize)) {
          size = stringType.ConstLength * charSize;
          return true;
        }

        break;
      }
      case ISequenceMemberType sequenceType: {
        if (sequenceType.LengthSourceType is SequenceLengthSourceType
                .CONST_LENGTH) {
          if (TryGetSizeOfType(sequenceType.ElementType, out var elementSize)) {
            size = (int) sequenceType.ConstLength * elementSize;
            return true;
          }
        }

        break;
      }
      case IContainerMemberType containerType: {
        return TryGetSizeOfContainer(
            (containerType.TypeSymbol as INamedTypeSymbol)!,
            out size);
      }
    }

    size = -1;
    return false;
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
    if (primitiveType is SchemaPrimitiveType.CHAR) {
      return TryGetEncodingCharLength(StringEncodingType.ASCII, out size);
    }

    if (primitiveType is SchemaPrimitiveType.BOOLEAN) {
      size = -1;
      return false;
    }

    var numberType = primitiveType.AsNumberType();
    if (numberType == SchemaNumberType.UNDEFINED) {
      size = -1;
      return false;
    }

    size = GetSize(numberType);
    return true;
  }

  public static bool TryGetEncodingCharLength(
      StringEncodingType encodingType,
      out int charSize) {
    if (encodingType is StringEncodingType.ASCII) {
      charSize = 1;
      return true;
    }

    charSize = default;
    return false;
  }

  public static bool TryGetSizeOfContainer(
      INamedTypeSymbol typeSymbol,
      out int size) {
    if (typeSymbol.IsInterface()) {
      size = -1;
      return false;
    }

    if (typeSymbol.TryGetConst(
            BinarySchemaOffsetsGenerator.SCHEMA_SIZE_NAME,
            out size)) {
      return true;
    }

    try {
      var container = BinarySchemaContainerParser.Instance.ParseContainer(
          BetterSymbol.FromType(typeSymbol));
      if (container.IsRecursive()) {
        size = -1;
        return false;
      }

      container.ToBinarySteps()
               .TryToGetMemberOffsetsAndContainerSize(
                   out _,
                   out var containerSize);
      if (containerSize != null) {
        size = containerSize.Value;
        return true;
      }
    } catch { }

    size = -1;
    return false;
  }
}