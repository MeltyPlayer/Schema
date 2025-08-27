using System;

namespace schema.binary;

public static class BlitExtensions {
  public static bool IsBlittable(
      this ISchemaMember member,
      out SchemaNumberType outNumberType,
      out uint outCount) {
    if (member is not ISchemaValueMember valueMember) {
      outNumberType = default;
      outCount = default;
      return false;
    }

    if (valueMember.AlignStart != null ||
        valueMember.Endianness != null ||
        valueMember.IsPosition ||
        valueMember.TrackStartAndEnd ||
        valueMember.IfBoolean != null ||
        valueMember.Offset != null ||
        (valueMember.MemberType is IStringType
                                   or IGenericMemberType
                                   or ISequenceMemberType)) {
      outNumberType = default;
      outCount = default;
      return false;
    }

    switch (valueMember.MemberType) {
      case IContainerMemberType containerMemberType1: {
        // TODO: Handle case where container is a nested struct
        outNumberType = default;
        outCount = default;
        return false;
      }
      case IFloatMemberType floatMemberType: {
        if (floatMemberType.FixedPointAttribute != null) {
          outNumberType = default;
          outCount = default;
          return false;
        }

        if (floatMemberType.UseAltFormat) {
          outNumberType = floatMemberType.AltFormat;
        } else {
          outNumberType = floatMemberType.PrimitiveType.AsNumberType();
        }

        outCount = 1;
        return true;
      }
      case IIntegerMemberType integerMemberType: {
        if (integerMemberType.SizeOfStream ||
            integerMemberType.AccessChainToSizeOf != null ||
            integerMemberType.LengthOfSequenceMembers != null ||
            integerMemberType.LengthOfStringMembers != null ||
            integerMemberType.PointerToAttribute != null) {
          outNumberType = default;
          outCount = default;
          return false;
        }

        if (integerMemberType.UseAltFormat) {
          outNumberType = integerMemberType.AltFormat;
        } else {
          outNumberType = integerMemberType.PrimitiveType.AsNumberType();
        }

        outCount = 1;
        return true;
      }
      case IPrimitiveMemberType primitiveMemberType: {
        if (primitiveMemberType.UseAltFormat) {
          outNumberType = primitiveMemberType.AltFormat;
        } else {
          outNumberType = primitiveMemberType.PrimitiveType.AsNumberType();
        }

        outCount = 1;
        return true;
      }
      case IKnownStructMemberType knownStructMemberType:
        switch (knownStructMemberType.KnownStruct) {
          case KnownStruct.VECTOR2: {
            outNumberType = SchemaNumberType.SINGLE;
            outCount = 2;
            return true;
          }
          case KnownStruct.VECTOR3: {
            outNumberType = SchemaNumberType.SINGLE;
            outCount = 3;
            return true;
          }
          case KnownStruct.QUATERNION:
          case KnownStruct.VECTOR4: {
            outNumberType = SchemaNumberType.SINGLE;
            outCount = 4;
            return true;
          }
          case KnownStruct.MATRIX4X4: {
            outNumberType = SchemaNumberType.SINGLE;
            outCount = 4 * 4;
            return true;
          }
          case KnownStruct.MATRIX3X2: {
            outNumberType = SchemaNumberType.SINGLE;
            outCount = 3 * 2;
            return true;
          }
          default: throw new ArgumentOutOfRangeException();
        }
      default: throw new ArgumentOutOfRangeException();
    }
  }
}