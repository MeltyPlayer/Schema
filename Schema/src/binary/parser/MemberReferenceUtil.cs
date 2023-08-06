using System;

using schema.binary.attributes;
using schema.util;

namespace schema.binary.parser {
  internal static class MemberReferenceUtil {
    public static IMemberType WrapTypeInfoWithMemberType(
        ITypeInfo memberTypeInfo) {
      switch (memberTypeInfo) {
        case IIntegerTypeInfo integerTypeInfo:
        case INumberTypeInfo numberTypeInfo:
        case IBoolTypeInfo boolTypeInfo:
        case ICharTypeInfo charTypeInfo:
        case IEnumTypeInfo enumTypeInfo: {
          return new BinarySchemaStructureParser.PrimitiveMemberType {
              PrimitiveTypeInfo =
                  Asserts.CastNonnull(memberTypeInfo as IPrimitiveTypeInfo),
          };
        }
        case IStringTypeInfo stringTypeInfo: {
          return new BinarySchemaStructureParser.StringType {
              TypeInfo = memberTypeInfo,
          };
        }
        case IStructureTypeInfo structureTypeInfo: {
          return new BinarySchemaStructureParser.StructureMemberType {
              StructureTypeInfo = structureTypeInfo,
          };
        }
        case IGenericTypeInfo genericTypeInfo: {
          // TODO: Figure out how to find the best constraint
          var constraintTypeInfo = genericTypeInfo.ConstraintTypeInfos[0];
          var constraintMemberType =
              MemberReferenceUtil
                  .WrapTypeInfoWithMemberType(constraintTypeInfo);

          return new BinarySchemaStructureParser.GenericMemberType {
              ConstraintType = constraintMemberType,
              GenericTypeInfo = genericTypeInfo,
          };
        }
        case ISequenceTypeInfo sequenceTypeInfo: {
          return new BinarySchemaStructureParser.SequenceMemberType {
              SequenceTypeInfo = sequenceTypeInfo,
              ElementType =
                  WrapTypeInfoWithMemberType(sequenceTypeInfo.ElementTypeInfo),
              LengthSourceType =
                  sequenceTypeInfo.IsLengthConst
                      ? SequenceLengthSourceType.READ_ONLY
                      : sequenceTypeInfo
                        .TypeV2
                        .HasAttribute<
                            RSequenceUntilEndOfStreamAttribute>()
                          ? SequenceLengthSourceType.UNTIL_END_OF_STREAM
                          : SequenceLengthSourceType.UNSPECIFIED,
          };
        }
        default: throw new ArgumentOutOfRangeException(nameof(memberTypeInfo));
      }
    }

    public static BinarySchemaStructureParser.SchemaValueMember
        WrapMemberReference(
            IMemberReference memberReference)
      => new() {
          Name = memberReference.Name,
          MemberType = MemberReferenceUtil.WrapTypeInfoWithMemberType(
              memberReference.MemberTypeInfo),
      };
  }
}