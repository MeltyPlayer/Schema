using System;
using System.Linq;
using System.Numerics;

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
      case IBoolTypeInfo boolTypeInfo:
      case ICharTypeInfo charTypeInfo:
      case IEnumTypeInfo enumTypeInfo: {
        return new BinarySchemaContainerParser.IntegerMemberType {
            PrimitiveTypeInfo =
                Asserts.CastNonnull(memberTypeInfo as IPrimitiveTypeInfo),
        };
      }
      case INumberTypeInfo numberTypeInfo: {
        return new BinarySchemaContainerParser.FloatMemberType {
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
        if (IsKnownStruct(containerTypeInfo.TypeSymbol, out var knownStruct)) {
          return new BinarySchemaContainerParser.KnownStructMemberType {
              ContainerTypeInfo = containerTypeInfo, KnownStruct = knownStruct
          };
        }

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

  public static bool IsKnownStruct(ITypeSymbol typeSymbol,
                                   out KnownStruct knownStruct) {
    if (typeSymbol.IsType<Vector2>()) {
      knownStruct = KnownStruct.VECTOR2;
      return true;
    }

    if (typeSymbol.IsType<Vector3>()) {
      knownStruct = KnownStruct.VECTOR3;
      return true;
    }

    if (typeSymbol.IsType<Vector4>()) {
      knownStruct = KnownStruct.VECTOR4;
      return true;
    }

    if (typeSymbol.IsType<Matrix3x2>()) {
      knownStruct = KnownStruct.MATRIX3X2;
      return true;
    }


    if (typeSymbol.IsType<Matrix4x4>()) {
      knownStruct = KnownStruct.MATRIX4X4;
      return true;
    }

    if (typeSymbol.IsType<Quaternion>()) {
      knownStruct = KnownStruct.QUATERNION;
      return true;
    }

    knownStruct = default;
    return false;
  }
}