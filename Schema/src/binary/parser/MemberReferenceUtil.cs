using System;
using System.Linq;

using Microsoft.CodeAnalysis;

using schema.binary.attributes;
using schema.util.asserts;
using schema.util.symbols;


namespace schema.binary.parser;

internal static class MemberReferenceUtil {
  public static INamedTypeSymbol BinaryConvertibleTypeSymbol {
    get;
    private set;
  }

  public static INamedTypeSymbol BinarySerializableTypeSymbol {
    get;
    private set;
  }

  public static INamedTypeSymbol BinaryDeserializableTypeSymbol {
    get;
    private set;
  }

  public static void PopulateBinaryTypes(Compilation compilation) {
      MemberReferenceUtil.BinaryConvertibleTypeSymbol
          = Asserts.CastNonnull(
              compilation.GetTypeByMetadataName(
                  "schema.binary.IBinaryConvertible"));
      MemberReferenceUtil.BinarySerializableTypeSymbol
          = Asserts.CastNonnull(
              compilation.GetTypeByMetadataName(
                  "schema.binary.IBinarySerializable"));
      MemberReferenceUtil.BinaryDeserializableTypeSymbol
          = Asserts.CastNonnull(
              compilation.GetTypeByMetadataName(
                  "schema.binary.IBinaryDeserializable"));
    }

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
          var rawConstraintTypeSymbols = rawConstraintTypeInfos
                                         .Select(
                                             typeInfo => typeInfo.TypeSymbol)
                                         .ToArray();

          var isBinarySerializable = rawConstraintTypeSymbols.Any(
              t => t.IsBinarySerializable());
          var isBinaryDeserializable = rawConstraintTypeSymbols.Any(
              t => t.IsBinaryDeserializable());

          ITypeSymbol? constraintTypeSymbol = null;
          if (isBinarySerializable && isBinaryDeserializable) {
            constraintTypeSymbol = BinaryConvertibleTypeSymbol;
          } else if (isBinarySerializable) {
            constraintTypeSymbol = BinarySerializableTypeSymbol;
          } else if (isBinaryDeserializable) {
            constraintTypeSymbol = BinaryDeserializableTypeSymbol;
          }

          IMemberType? constraintMemberType = null;
          if (constraintTypeSymbol != null) {
            new TypeInfoParser().ParseTypeSymbol(constraintTypeSymbol,
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
                        .TypeSymbol
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