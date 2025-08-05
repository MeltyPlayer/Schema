using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

using schema.util.asserts;
using schema.util.symbols;


namespace schema.binary.parser;

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
  ITypeSymbol TypeSymbol { get; }
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
            out var isReadonly)) {
      return ParseStatus.NOT_A_FIELD_OR_PROPERTY_OR_METHOD;
    }

    // Primary constructor params.
    var memberName = memberSymbol.Name;
    if (memberName.Contains('<') || memberName.Contains('>')) {
      return ParseStatus.NOT_A_FIELD_OR_PROPERTY_OR_METHOD;
    }

    return this.ParseTypeSymbol(
        memberTypeSymbol,
        isReadonly,
        out memberTypeInfo);
  }

  public ParseStatus ParseTypeSymbol(
      ITypeSymbol typeSymbol,
      bool isReadonly,
      out ITypeInfo typeInfo) {
    this.ParseNullable_(ref typeSymbol, out var isNullable);

    if (typeSymbol.IsPrimitive(out var primitiveType)) {
      switch (primitiveType) {
        case SchemaPrimitiveType.BOOLEAN: {
          typeInfo = new BoolTypeInfo(
              typeSymbol,
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
              typeSymbol,
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
              typeSymbol,
              SchemaTypeKind.FLOAT,
              primitiveType.AsNumberType(),
              isReadonly,
              isNullable);
          return ParseStatus.SUCCESS;
        }
        case SchemaPrimitiveType.CHAR: {
          typeInfo = new CharTypeInfo(
              typeSymbol,
              isReadonly,
              isNullable);
          return ParseStatus.SUCCESS;
        }
        case SchemaPrimitiveType.ENUM: {
          typeInfo = new EnumTypeInfo(
              typeSymbol,
              isReadonly,
              isNullable);
          return ParseStatus.SUCCESS;
        }
        default: throw new ArgumentOutOfRangeException();
      }
    }

    if (typeSymbol.IsString()) {
      typeInfo = new StringTypeInfo(
          typeSymbol,
          isReadonly,
          isNullable);
      return ParseStatus.SUCCESS;
    }

    if (typeSymbol.IsSequence(out var elementTypeV2, out var sequenceType)) {
      var elementParseStatus = this.ParseTypeSymbol(
          elementTypeV2,
          sequenceType.IsReadOnly(),
          out var elementTypeInfo);
      if (elementParseStatus != ParseStatus.SUCCESS) {
        typeInfo = default;
        return elementParseStatus;
      }

      typeInfo = new SequenceTypeInfo(
          typeSymbol,
          isReadonly,
          isNullable,
          sequenceType,
          isReadonly && sequenceType.IsConstLength(),
          elementTypeInfo);
      return ParseStatus.SUCCESS;
    }

    if (typeSymbol.IsClass() ||
        typeSymbol.IsInterface() ||
        typeSymbol.IsStruct() ||
        typeSymbol is IErrorTypeSymbol) {
      typeInfo = new ContainerTypeInfo(
          typeSymbol,
          isReadonly,
          isNullable);
      return ParseStatus.SUCCESS;
    }

    if (typeSymbol.IsGenericTypeParameter(out var typeParameterSymbol)) {
      var constraintTypeInfos =
          typeParameterSymbol
              .ConstraintTypes
              .Where(t => t is not IErrorTypeSymbol)
              .Select(constraintType => {
                        var parseStatus = this.ParseTypeSymbol(
                            constraintType,
                            isReadonly,
                            out var constraintTypeInfo);
                        Asserts.Equal(ParseStatus.SUCCESS, parseStatus);
                        return constraintTypeInfo;
                      })
              .ToArray();

      typeInfo = new GenericTypeInfo(
          constraintTypeInfos,
          typeSymbol,
          isReadonly,
          isNullable);
      return ParseStatus.SUCCESS;
    }

    typeInfo = default;
    return ParseStatus.NOT_IMPLEMENTED;
  }

  public ITypeInfo AssertParseType(ITypeSymbol typeSymbol) {
    var parseStatus
        = this.ParseTypeSymbol(typeSymbol, true, out var typeInfo);
    if (parseStatus != ParseStatus.SUCCESS) {
      throw new NotImplementedException();
    }

    return typeInfo;
  }

  private bool GetTypeOfMember_(
      ISymbol memberSymbol,
      out ITypeSymbol memberTypeSymbol,
      out bool isMemberReadonly) {
    switch (memberSymbol) {
      case IPropertySymbol propertySymbol: {
        isMemberReadonly = propertySymbol.SetMethod == null;
        memberTypeSymbol = propertySymbol.Type;
        return true;
      }
      case IFieldSymbol fieldSymbol: {
        isMemberReadonly = fieldSymbol.IsReadOnly;
        memberTypeSymbol = fieldSymbol.Type;
        return true;
      }
      default: {
        isMemberReadonly = false;
        memberTypeSymbol = default;
        return false;
      }
    }
  }

  private void ParseNullable_(ref ITypeSymbol typeSymbol,
                              out bool isNullable) {
    isNullable = false;
    if (typeSymbol.IsType(typeof(Nullable<>))) {
      Asserts.True(typeSymbol.IsGeneric(out _, out var genericArguments));
      typeSymbol = genericArguments.ToArray()[0];
      isNullable = true;
    } else if (typeSymbol.IsNullable(out _)) {
      isNullable = true;
    }
  }

  private record BoolTypeInfo(
      ITypeSymbol TypeSymbol,
      bool IsReadOnly,
      bool IsNullable) : IBoolTypeInfo {
    public ITypeSymbol TypeSymbol { get; } = TypeSymbol;

    public SchemaPrimitiveType PrimitiveType => SchemaPrimitiveType.BOOLEAN;
    public SchemaTypeKind Kind => SchemaTypeKind.BOOL;

    public bool IsReadOnly { get; } = IsReadOnly;
    public bool IsNullable { get; } = IsNullable;
  }


  private class FloatTypeInfo(
      ITypeSymbol typeSymbol,
      SchemaTypeKind kind,
      SchemaNumberType numberType,
      bool isReadonly,
      bool isNullable)
      : INumberTypeInfo {
    public ITypeSymbol TypeSymbol { get; } = typeSymbol;
    public SchemaTypeKind Kind { get; } = kind;
    public SchemaNumberType NumberType { get; } = numberType;

    public SchemaPrimitiveType PrimitiveType
      => this.NumberType.AsPrimitiveType();

    public bool IsReadOnly { get; } = isReadonly;
    public bool IsNullable { get; } = isNullable;
  }

  private record IntegerTypeInfo(
      ITypeSymbol TypeSymbol,
      SchemaTypeKind Kind,
      SchemaIntegerType IntegerType,
      bool IsReadOnly,
      bool IsNullable) : IIntegerTypeInfo {
    public ITypeSymbol TypeSymbol { get; } = TypeSymbol;
    public SchemaTypeKind Kind { get; } = Kind;
    public SchemaIntegerType IntegerType { get; } = IntegerType;

    public SchemaNumberType NumberType => this.IntegerType.AsNumberType();

    public SchemaPrimitiveType PrimitiveType
      => this.NumberType.AsPrimitiveType();

    public bool IsReadOnly { get; } = IsReadOnly;
    public bool IsNullable { get; } = IsNullable;
  }

  private record CharTypeInfo(
      ITypeSymbol TypeSymbol,
      bool IsReadOnly,
      bool IsNullable) : ICharTypeInfo {
    public SchemaPrimitiveType PrimitiveType => SchemaPrimitiveType.CHAR;
    public SchemaTypeKind Kind => SchemaTypeKind.CHAR;

    public ITypeSymbol TypeSymbol { get; } = TypeSymbol;

    public bool IsReadOnly { get; } = IsReadOnly;
    public bool IsNullable { get; } = IsNullable;
  }

  private record StringTypeInfo(
      ITypeSymbol TypeSymbol,
      bool IsReadOnly,
      bool IsNullable) : IStringTypeInfo {
    public SchemaTypeKind Kind => SchemaTypeKind.STRING;

    public ITypeSymbol TypeSymbol { get; } = TypeSymbol;

    public bool IsReadOnly { get; } = IsReadOnly;
    public bool IsNullable { get; } = IsNullable;
  }

  private class EnumTypeInfo(
      ITypeSymbol typeSymbol,
      bool isReadonly,
      bool isNullable)
      : IEnumTypeInfo {
    public SchemaPrimitiveType PrimitiveType => SchemaPrimitiveType.ENUM;
    public SchemaTypeKind Kind => SchemaTypeKind.ENUM;

    public ITypeSymbol TypeSymbol { get; } = typeSymbol;

    public bool IsReadOnly { get; } = isReadonly;
    public bool IsNullable { get; } = isNullable;
  }

  private class ContainerTypeInfo(
      ITypeSymbol typeSymbol,
      bool isReadonly,
      bool isNullable)
      : IContainerTypeInfo {
    public SchemaTypeKind Kind => SchemaTypeKind.CONTAINER;

    public ITypeSymbol TypeSymbol { get; } = typeSymbol;

    public bool IsReadOnly { get; } = isReadonly;
    public bool IsNullable { get; } = isNullable;
  }

  private class GenericTypeInfo(
      ITypeInfo[] constraintTypeInfos,
      ITypeSymbol typeSymbol,
      bool isReadonly,
      bool isNullable)
      : IGenericTypeInfo {
    public SchemaTypeKind Kind => SchemaTypeKind.GENERIC;

    public ITypeInfo[] ConstraintTypeInfos { get; } = constraintTypeInfos;
    public ITypeSymbol TypeSymbol { get; } = typeSymbol;

    public bool IsReadOnly { get; } = isReadonly;
    public bool IsNullable { get; } = isNullable;
  }

  private class SequenceTypeInfo(
      ITypeSymbol typeSymbol,
      bool isReadonly,
      bool isNullable,
      SequenceType sequenceType,
      bool isLengthConst,
      ITypeInfo containedType) : ISequenceTypeInfo {
    public SchemaTypeKind Kind => SchemaTypeKind.SEQUENCE;

    public ITypeSymbol TypeSymbol { get; } = typeSymbol;

    public bool IsReadOnly { get; } = isReadonly;
    public bool IsNullable { get; } = isNullable;

    public SequenceType SequenceType { get; } = sequenceType;
    public bool IsLengthConst { get; } = isLengthConst;
    public ITypeInfo ElementTypeInfo { get; } = containedType;

    public string LengthName
      => this.SequenceType is SequenceType.MUTABLE_ARRAY
                              or SequenceType.IMMUTABLE_ARRAY
          ? "Length"
          : "Count";
  }
}