using System;
using System.Linq;

using schema.binary.attributes;
using schema.util;
using schema.util.asserts;
using schema.util.types;

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
          return new BinarySchemaContainerParser.PrimitiveMemberType {
              PrimitiveTypeInfo =
                  Asserts.CastNonnull(memberTypeInfo as IPrimitiveTypeInfo),
          };
        }
        case IStringTypeInfo stringTypeInfo: {
          return new BinarySchemaContainerParser.StringType {
              TypeInfo = memberTypeInfo,
          };
        }
        case IContainerTypeInfo containerTypeInfo: {
          return new BinarySchemaContainerParser.ContainerMemberType {
              ContainerTypeInfo = containerTypeInfo,
          };
        }
        case IGenericTypeInfo genericTypeInfo: {
          var rawConstraintTypeInfos = genericTypeInfo.ConstraintTypeInfos;
          var rawConstraintTypeVs2s = rawConstraintTypeInfos
                                       .Select(typeInfo => typeInfo.TypeV2)
                                       .ToArray();

          var isBinarySerializable = rawConstraintTypeVs2s.Any(
              typeV2 => typeV2.IsBinarySerializable);
          var isBinaryDeserializable = rawConstraintTypeVs2s.Any(
              typeV2 => typeV2.IsBinaryDeserializable);

          ITypeV2? constraintTypeV2 = null;
          if (isBinarySerializable && isBinaryDeserializable) {
            constraintTypeV2 = TypeV2.FromType<IBinaryConvertible>();
          } else if (isBinarySerializable) {
            constraintTypeV2 = TypeV2.FromType<IBinarySerializable>();
          } else if (isBinaryDeserializable) {
            constraintTypeV2 = TypeV2.FromType<IBinaryDeserializable>();
          }

          IMemberType? constraintMemberType = null;
          if (constraintTypeV2 != null) {
            new TypeInfoParser().ParseTypeV2(constraintTypeV2,
                                             genericTypeInfo.IsReadOnly,
                                             out var constraintTypeInfo);
            constraintMemberType =
                MemberReferenceUtil
                    .WrapTypeInfoWithMemberType(constraintTypeInfo);
          }

          return new BinarySchemaContainerParser.GenericMemberType {
              ConstraintType = constraintMemberType,
              GenericTypeInfo = genericTypeInfo,
          };
        }
        case ISequenceTypeInfo sequenceTypeInfo: {
          return new BinarySchemaContainerParser.SequenceMemberType {
              SequenceTypeInfo = sequenceTypeInfo,
              ElementType =
                  WrapTypeInfoWithMemberType(sequenceTypeInfo.ElementTypeInfo),
              LengthSourceType =
                  sequenceTypeInfo.IsLengthConst
                      ? SequenceLengthSourceType.READ_ONLY
                      : sequenceTypeInfo
                        .TypeV2
                        .HasAttribute<RSequenceUntilEndOfStreamAttribute>()
                          ? SequenceLengthSourceType.UNTIL_END_OF_STREAM
                          : SequenceLengthSourceType.UNSPECIFIED,
          };
        }

        default: throw new ArgumentOutOfRangeException(nameof(memberTypeInfo));
      }
    }

    public static BinarySchemaContainerParser.SchemaValueMember
        WrapMemberReference(
            IMemberReference memberReference)
      => new() {
          Name = memberReference.Name,
          MemberType = MemberReferenceUtil.WrapTypeInfoWithMemberType(
              memberReference.MemberTypeInfo),
      };
  }
}