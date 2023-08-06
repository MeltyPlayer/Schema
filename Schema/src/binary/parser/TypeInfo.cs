using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

using schema.util;
using schema.util.symbols;
using schema.util.types;


namespace schema.binary.parser {
  public enum SchemaTypeKind {
    BOOL,
    INTEGER,
    FLOAT,
    CHAR,
    STRING,
    ENUM,
    CONTAINER,
    GENERIC,
    SEQUENCE,
  }

  public interface ITypeInfo {
    ITypeV2 TypeV2 { get; }
    SchemaTypeKind Kind { get; }
    bool IsReadOnly { get; }
    bool IsNullable { get; }
  }

  public interface IPrimitiveTypeInfo : ITypeInfo {
    SchemaPrimitiveType PrimitiveType { get; }
  }

  public interface IBoolTypeInfo : IPrimitiveTypeInfo { }

  public interface INumberTypeInfo : IPrimitiveTypeInfo {
    SchemaNumberType NumberType { get; }
  }

  public interface IIntegerTypeInfo : INumberTypeInfo {
    SchemaIntegerType IntegerType { get; }
  }

  public interface IEnumTypeInfo : IPrimitiveTypeInfo { }

  public interface ICharTypeInfo : IPrimitiveTypeInfo { }

  public interface IStringTypeInfo : ITypeInfo { }

  public interface IContainerTypeInfo : ITypeInfo { }

  public interface IGenericTypeInfo : ITypeInfo {
    ITypeInfo[] ConstraintTypeInfos { get; }
  }

  public interface ISequenceTypeInfo : ITypeInfo {
    SequenceType SequenceType { get; }
    bool IsLengthConst { get; }
    ITypeInfo ElementTypeInfo { get; }

    string LengthName { get; }
  }

  public class TypeInfoParser {
    public enum ParseStatus {
      SUCCESS,
      NOT_A_FIELD_OR_PROPERTY_OR_METHOD,
      NOT_IMPLEMENTED,
    }

    public IEnumerable<(ParseStatus, ISymbol, ITypeSymbol, ITypeInfo?)>
        ParseMembers(
            INamedTypeSymbol containerSymbol) {
      foreach (var memberSymbol in containerSymbol.GetInstanceMembers()) {
        // Tries to parse the type to get info about it
        var parseStatus = this.ParseMember(
            memberSymbol,
            out var memberTypeSymbol,
            out var memberTypeInfo);
        yield return (parseStatus, memberSymbol, memberTypeSymbol,
                      memberTypeInfo);
      }
    }

    public ParseStatus ParseMember(ISymbol memberSymbol,
                                   out ITypeSymbol? memberTypeSymbol,
                                   out ITypeInfo? memberTypeInfo) {
      memberTypeSymbol = null;
      memberTypeInfo = null;

      if (memberSymbol is IMethodSymbol) {
        return ParseStatus.SUCCESS;
      }

      if (!GetTypeOfMember_(
              memberSymbol,
              out memberTypeSymbol,
              out var memberTypeV2,
              out var isReadonly)) {
        return ParseStatus.NOT_A_FIELD_OR_PROPERTY_OR_METHOD;
      }

      return this.ParseTypeV2(
          memberTypeV2,
          isReadonly,
          out memberTypeInfo);
    }

    public ParseStatus ParseTypeV2(
        ITypeV2 typeV2,
        bool isReadonly,
        out ITypeInfo typeInfo) {
      this.ParseNullable_(ref typeV2, out var isNullable);

      if (typeV2.IsPrimitive(out var primitiveType)) {
        switch (primitiveType) {
          case SchemaPrimitiveType.BOOLEAN: {
            typeInfo = new BoolTypeInfo(
                typeV2,
                isReadonly,
                isNullable);
            return ParseStatus.SUCCESS;
          }
          case SchemaPrimitiveType.BYTE:
          case SchemaPrimitiveType.SBYTE:
          case SchemaPrimitiveType.INT16:
          case SchemaPrimitiveType.UINT16:
          case SchemaPrimitiveType.INT32:
          case SchemaPrimitiveType.UINT32:
          case SchemaPrimitiveType.INT64:
          case SchemaPrimitiveType.UINT64: {
            typeInfo = new IntegerTypeInfo(
                typeV2,
                SchemaTypeKind.INTEGER,
                primitiveType.AsIntegerType(),
                isReadonly,
                isNullable);
            return ParseStatus.SUCCESS;
          }
          case SchemaPrimitiveType.SN8:
          case SchemaPrimitiveType.UN8:
          case SchemaPrimitiveType.SN16:
          case SchemaPrimitiveType.UN16:
          case SchemaPrimitiveType.SINGLE:
          case SchemaPrimitiveType.DOUBLE: {
            typeInfo = new FloatTypeInfo(
                typeV2,
                SchemaTypeKind.FLOAT,
                primitiveType.AsNumberType(),
                isReadonly,
                isNullable);
            return ParseStatus.SUCCESS;
          }
          case SchemaPrimitiveType.CHAR: {
            typeInfo = new CharTypeInfo(
                typeV2,
                isReadonly,
                isNullable);
            return ParseStatus.SUCCESS;
          }
          case SchemaPrimitiveType.ENUM: {
            typeInfo = new EnumTypeInfo(
                typeV2,
                isReadonly,
                isNullable);
            return ParseStatus.SUCCESS;
          }
          default: throw new ArgumentOutOfRangeException();
        }
      }

      if (typeV2.IsString) {
        typeInfo = new StringTypeInfo(
            typeV2,
            isReadonly,
            isNullable);
        return ParseStatus.SUCCESS;
      }

      if (typeV2.IsSequence(out var elementTypeV2, out var sequenceType)) {
        var elementParseStatus = this.ParseTypeV2(
            elementTypeV2,
            sequenceType.IsReadOnly(),
            out var elementTypeInfo);
        if (elementParseStatus != ParseStatus.SUCCESS) {
          typeInfo = default;
          return elementParseStatus;
        }

        typeInfo = new SequenceTypeInfo(
            typeV2,
            isReadonly,
            isNullable,
            sequenceType,
            isReadonly && sequenceType.IsConstLength(),
            elementTypeInfo);
        return ParseStatus.SUCCESS;
      }

      if (typeV2.IsClass || typeV2.IsInterface || typeV2.IsStruct) {
        typeInfo = new ContainerTypeInfo(
            typeV2,
            isReadonly,
            isNullable);
        return ParseStatus.SUCCESS;
      }

      if (typeV2.IsGenericTypeParameter(out var genericConstraints)) {
        var constraintTypeInfos =
            genericConstraints
                .Select(constraintType => {
                  var parseStatus = this.ParseTypeV2(
                      constraintType,
                      isReadonly,
                      out var constraintTypeInfo);
                  Asserts.Equal(ParseStatus.SUCCESS, parseStatus);
                  return constraintTypeInfo;
                })
                .ToArray();

        typeInfo = new GenericTypeInfo(
            constraintTypeInfos,
            typeV2,
            isReadonly,
            isNullable);
        return ParseStatus.SUCCESS;
      }

      typeInfo = default;
      return ParseStatus.NOT_IMPLEMENTED;
    }

    public ITypeInfo AssertParseTypeV2(
        ITypeV2 typeV2) {
      var parseStatus = this.ParseTypeV2(typeV2, true, out var typeInfo);
      if (parseStatus != ParseStatus.SUCCESS) {
        throw new NotImplementedException();
      }

      return typeInfo;
    }

    private bool GetTypeOfMember_(
        ISymbol memberSymbol,
        out ITypeSymbol memberTypeSymbol,
        out ITypeV2 memberTypeV2,
        out bool isMemberReadonly) {
      switch (memberSymbol) {
        case IPropertySymbol propertySymbol: {
          isMemberReadonly = propertySymbol.SetMethod == null;
          memberTypeSymbol = propertySymbol.Type;
          memberTypeV2 = TypeV2.FromSymbol(memberTypeSymbol);
          return true;
        }
        case IFieldSymbol fieldSymbol: {
          isMemberReadonly = fieldSymbol.IsReadOnly;
          memberTypeSymbol = fieldSymbol.Type;
          memberTypeV2 = TypeV2.FromSymbol(memberTypeSymbol);
          return true;
        }
        default: {
          isMemberReadonly = false;
          memberTypeSymbol = default;
          memberTypeV2 = default;
          return false;
        }
      }
    }

    private void ParseNullable_(ref ITypeV2 typeV2, out bool isNullable) {
      isNullable = false;
      if (typeV2.IsExactly(typeof(Nullable<>))) {
        Asserts.True(typeV2.HasGenericArguments(out var genericArguments));
        typeV2 = genericArguments.ToArray()[0];
        isNullable = true;
      } else if (typeV2.HasNullableAnnotation) {
        isNullable = true;
      }
    }

    private record BoolTypeInfo(ITypeV2 TypeV2,
                                bool IsReadOnly,
                                bool IsNullable) :
        IBoolTypeInfo {
      public ITypeV2 TypeV2 { get; } = TypeV2;

      public SchemaPrimitiveType PrimitiveType => SchemaPrimitiveType.BOOLEAN;
      public SchemaTypeKind Kind => SchemaTypeKind.BOOL;

      public bool IsReadOnly { get; } = IsReadOnly;
      public bool IsNullable { get; } = IsNullable;
    }


    private class FloatTypeInfo(ITypeV2 typeV2,
                                SchemaTypeKind kind,
                                SchemaNumberType numberType,
                                bool isReadonly,
                                bool isNullable)
        : INumberTypeInfo {
      public ITypeV2 TypeV2 { get; } = typeV2;
      public SchemaTypeKind Kind { get; } = kind;
      public SchemaNumberType NumberType { get; } = numberType;

      public SchemaPrimitiveType PrimitiveType
        => this.NumberType.AsPrimitiveType();

      public bool IsReadOnly { get; } = isReadonly;
      public bool IsNullable { get; } = isNullable;
    }

    private record IntegerTypeInfo(ITypeV2 TypeV2,
                                   SchemaTypeKind Kind,
                                   SchemaIntegerType IntegerType,
                                   bool IsReadOnly,
                                   bool IsNullable) : IIntegerTypeInfo {
      public ITypeV2 TypeV2 { get; } = TypeV2;
      public SchemaTypeKind Kind { get; } = Kind;
      public SchemaIntegerType IntegerType { get; } = IntegerType;

      public SchemaNumberType NumberType => this.IntegerType.AsNumberType();

      public SchemaPrimitiveType PrimitiveType
        => this.NumberType.AsPrimitiveType();

      public bool IsReadOnly { get; } = IsReadOnly;
      public bool IsNullable { get; } = IsNullable;
    }

    private record CharTypeInfo(ITypeV2 TypeV2,
                                bool IsReadOnly,
                                bool IsNullable) : ICharTypeInfo {
      public SchemaPrimitiveType PrimitiveType => SchemaPrimitiveType.CHAR;
      public SchemaTypeKind Kind => SchemaTypeKind.CHAR;

      public ITypeV2 TypeV2 { get; } = TypeV2;

      public bool IsReadOnly { get; } = IsReadOnly;
      public bool IsNullable { get; } = IsNullable;
    }

    private record StringTypeInfo(ITypeV2 TypeV2,
                                  bool IsReadOnly,
                                  bool IsNullable) : IStringTypeInfo {
      public SchemaTypeKind Kind => SchemaTypeKind.STRING;

      public ITypeV2 TypeV2 { get; } = TypeV2;

      public bool IsReadOnly { get; } = IsReadOnly;
      public bool IsNullable { get; } = IsNullable;
    }

    private class EnumTypeInfo(ITypeV2 typeV2,
                               bool isReadonly,
                               bool isNullable)
        : IEnumTypeInfo {
      public SchemaPrimitiveType PrimitiveType => SchemaPrimitiveType.ENUM;
      public SchemaTypeKind Kind => SchemaTypeKind.ENUM;

      public ITypeV2 TypeV2 { get; } = typeV2;

      public bool IsReadOnly { get; } = isReadonly;
      public bool IsNullable { get; } = isNullable;
    }

    private class ContainerTypeInfo(ITypeV2 typeV2,
                                    bool isReadonly,
                                    bool isNullable)
        : IContainerTypeInfo {
      public SchemaTypeKind Kind => SchemaTypeKind.CONTAINER;

      public ITypeV2 TypeV2 { get; } = typeV2;

      public bool IsReadOnly { get; } = isReadonly;
      public bool IsNullable { get; } = isNullable;
    }

    private class GenericTypeInfo(ITypeInfo[] constraintTypeInfos,
                                  ITypeV2 typeV2,
                                  bool isReadonly,
                                  bool isNullable)
        : IGenericTypeInfo {
      public SchemaTypeKind Kind => SchemaTypeKind.GENERIC;

      public ITypeInfo[] ConstraintTypeInfos { get; } = constraintTypeInfos;
      public ITypeV2 TypeV2 { get; } = typeV2;

      public bool IsReadOnly { get; } = isReadonly;
      public bool IsNullable { get; } = isNullable;
    }

    private class SequenceTypeInfo : ISequenceTypeInfo {
      public SequenceTypeInfo(
          ITypeV2 typeV2,
          bool isReadonly,
          bool isNullable,
          SequenceType sequenceType,
          bool isLengthConst,
          ITypeInfo containedType) {
        this.TypeV2 = typeV2;
        this.IsReadOnly = isReadonly;
        this.IsNullable = isNullable;
        this.SequenceType = sequenceType;
        this.IsLengthConst = isLengthConst;
        this.ElementTypeInfo = containedType;
      }

      public SchemaTypeKind Kind => SchemaTypeKind.SEQUENCE;

      public ITypeV2 TypeV2 { get; }

      public bool IsReadOnly { get; }
      public bool IsNullable { get; }

      public SequenceType SequenceType { get; }
      public bool IsLengthConst { get; }
      public ITypeInfo ElementTypeInfo { get; }

      public string LengthName
        => this.SequenceType is SequenceType.MUTABLE_ARRAY
                                or SequenceType.IMMUTABLE_ARRAY
            ? "Length"
            : "Count";
    }
  }
}