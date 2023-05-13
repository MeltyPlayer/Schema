using System;

using schema.binary.attributes;
using schema.binary.attributes.sequence;
using schema.binary.util;

namespace schema.binary.parser {
  internal static class MemberReferenceUtil {
    public static IMemberType WrapTypeInfoWithMemberType(ITypeInfo typeInfo) {
      switch (typeInfo) {
        case IIntegerTypeInfo integerTypeInfo:
        case INumberTypeInfo numberTypeInfo:
        case IBoolTypeInfo boolTypeInfo:
        case ICharTypeInfo charTypeInfo:
        case IEnumTypeInfo enumTypeInfo: {
            return new BinarySchemaStructureParser.PrimitiveMemberType {
              PrimitiveTypeInfo =
                    Asserts.CastNonnull(typeInfo as IPrimitiveTypeInfo),
            };
          }
        case IStringTypeInfo stringTypeInfo: {
            return new BinarySchemaStructureParser.StringType { TypeInfo = typeInfo, };
          }
        case IStructureTypeInfo structureTypeInfo: {
            return new BinarySchemaStructureParser.StructureMemberType {
              StructureTypeInfo = structureTypeInfo,
              IsReferenceType =
                    structureTypeInfo.NamedTypeSymbol.IsReferenceType,
            };
          }
        case IGenericTypeInfo genericTypeInfo: {
            // TODO: Figure out how to find the best constraint
            var constraintTypeInfo = genericTypeInfo.ConstraintTypeInfos[0];
            var constraintMemberType =
                MemberReferenceUtil.WrapTypeInfoWithMemberType(constraintTypeInfo);

            return new BinarySchemaStructureParser.GenericMemberType {
              ConstraintType = constraintMemberType,
              GenericTypeInfo = genericTypeInfo,
            };
          }
        case ISequenceTypeInfo sequenceTypeInfo: {
            return new BinarySchemaStructureParser.SequenceMemberType {
              SequenceTypeInfo = sequenceTypeInfo,
              SequenceType = sequenceTypeInfo.IsArray
                                   ? SequenceType.ARRAY
                                   : SequenceType.LIST,
              ElementType =
                    WrapTypeInfoWithMemberType(sequenceTypeInfo.ElementTypeInfo),
              LengthSourceType =
                    sequenceTypeInfo.IsLengthConst
                        ? SequenceLengthSourceType.READONLY
                        : ((ISequenceLengthSourceAttribute?) SymbolTypeUtil
                            .GetAttribute<SequenceLengthSourceAttribute>(
                                null,
                                sequenceTypeInfo.TypeSymbol) ?? SymbolTypeUtil
                            .GetAttribute<RSequenceLengthSourceAttribute>(
                                null,
                                sequenceTypeInfo.TypeSymbol)) == null
                            ? SequenceLengthSourceType.UNSPECIFIED
                            : SequenceLengthSourceType.UNTIL_END_OF_STREAM,
            };
          }
        default: throw new ArgumentOutOfRangeException(nameof(typeInfo));
      }
    }

    public static BinarySchemaStructureParser.SchemaMember WrapMemberReference(IMemberReference memberReference)
      => new() {
        Name = memberReference.Name,
        MemberType = MemberReferenceUtil.WrapTypeInfoWithMemberType(
              memberReference.MemberTypeInfo),
      };
  }
}
