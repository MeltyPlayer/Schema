using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using schema.binary;
using schema.util.enumerables;

namespace schema.util.types {
  public static partial class TypeV2 {
    public static ITypeV2 FromType<T>() => FromType(typeof(T));

    public static ITypeV2 FromType(Type type)
      => new TypeBasedTypeV2(type);

    private class TypeBasedTypeV2 : BSymbolTypeV2 {
      private readonly Type type_;

      public TypeBasedTypeV2(Type type) {
        this.type_ = type;
      }

      public override string Name => this.type_.Name;

      public override string? FullyQualifiedNamespace => this.type_.Namespace;

      public override IEnumerable<string> NamespaceParts
        => this.type_.Namespace?.Split('.') ?? Enumerable.Empty<string>();

      public override IEnumerable<string> DeclaringTypeNamesDownward
        => DeclaringTypeNamesUpward.Reverse();

      private IEnumerable<string> DeclaringTypeNamesUpward {
        get {
          var declaringType = this.type_.DeclaringType;
          while (declaringType != null) {
            yield return declaringType.Name;
            declaringType = declaringType.DeclaringType;
          }
        }
      }

      public override bool Implements(Type other, out ITypeV2 matchingType) {
        var matchingTypeImpl = this.type_.Yield()
                                   .Concat(this.type_.GetInterfaces())
                                   .Concat(BaseTypes)
                                   .SingleOrDefault(type => type == other);
        matchingType = matchingTypeImpl != null
            ? TypeV2.FromType(matchingTypeImpl)
            : null;
        return matchingType != null;
      }

      private IEnumerable<Type> BaseTypes {
        get {
          var baseType = this.type_.BaseType;
          while (baseType != null) {
            yield return baseType;
            baseType = baseType.BaseType;
          }
        }
      }

      public override int Arity
        => this.type_.IsGenericParameter
            ? this.type_.GetGenericParameterConstraints().Length
            : 0;

      public override bool IsClass => this.type_.IsClass;

      public override bool IsInterface => this.type_.IsInterface;

      public override bool IsStruct => this.type_ is
          { IsValueType: true, IsEnum: false };

      public override bool IsString => this.type_ == typeof(string);

      public override bool IsArray(out ITypeV2 elementType) {
        var elementTypeImpl = this.type_.GetElementType();
        elementType = elementTypeImpl != null
            ? TypeV2.FromType(elementTypeImpl)
            : null;
        return this.type_.IsArray;
      }

      public override bool IsPrimitive(
          out SchemaPrimitiveType primitiveType) {
        primitiveType = this.GetPrimitiveType_(this.type_);
        return primitiveType != SchemaPrimitiveType.UNDEFINED;
      }

      public override bool IsEnum(
          out SchemaIntegerType underlyingType) {
        underlyingType =
            this.GetPrimitiveType_(this.type_.GetEnumUnderlyingType())
                .AsIntegerType();
        return underlyingType != SchemaIntegerType.UNDEFINED;
      }


      public override bool ContainsMemberWithType(ITypeV2 other) {
        var fieldTypeV2s =
            this.type_.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Concat(
                    this.type_.GetFields(BindingFlags.NonPublic |
                                         BindingFlags.Instance))
                .Where(field => !field.Name.Contains("k__BackingField"))
                .Select(field => field.FieldType)
                .Distinct()
                .Select(TypeV2.FromType);
        var propertyTypeV2s =
            this.type_.GetProperties(
                    BindingFlags.Public | BindingFlags.Instance)
                .Concat(
                    this.type_.GetProperties(BindingFlags.NonPublic |
                                             BindingFlags.Instance))
                .Where(property => property.GetIndexParameters().Length == 0)
                .Select(property => property.PropertyType)
                .Distinct()
                .Select(TypeV2.FromType);

        return fieldTypeV2s
               .Concat(propertyTypeV2s)
               .Select(typeV2 => typeV2.IsSequence(out var elementTypeV2, out _)
                           ? elementTypeV2
                           : typeV2)
               .Any(other.IsExactly);
      }

      public override bool HasAttribute<TAttribute>()
        => this.GetAttribute<TAttribute>() != null;

      public override TAttribute GetAttribute<TAttribute>()
        => this.GetAttributes<TAttribute>().Single();

      public override IEnumerable<TAttribute> GetAttributes<TAttribute>()
        => this.type_.GetCustomAttributes<TAttribute>();

      public override bool HasGenericArguments(
          out IEnumerable<ITypeV2> genericArguments) {
        if (this.Arity == 0) {
          genericArguments = default;
          return false;
        }

        genericArguments =
            this.type_.GenericTypeArguments.Select(TypeV2.FromType);
        return true;
      }

      public override bool IsGenericTypeParameter(
          out IEnumerable<ITypeV2> genericConstraints) {
        if (!this.type_.IsGenericParameter) {
          genericConstraints = default;
          return false;
        }

        genericConstraints = this.type_.GetGenericParameterConstraints()
                                 .Select(TypeV2.FromType);
        return true;
      }

      private SchemaPrimitiveType GetPrimitiveType_(Type type) {
        if (type.IsEnum) {
          return SchemaPrimitiveType.ENUM;
        }

        if (type == typeof(bool)) return SchemaPrimitiveType.BOOLEAN;
        if (type == typeof(char)) return SchemaPrimitiveType.CHAR;
        if (type == typeof(byte)) return SchemaPrimitiveType.BYTE;
        if (type == typeof(sbyte)) return SchemaPrimitiveType.SBYTE;
        if (type == typeof(short)) return SchemaPrimitiveType.INT16;
        if (type == typeof(ushort)) return SchemaPrimitiveType.UINT16;
        if (type == typeof(int)) return SchemaPrimitiveType.INT32;
        if (type == typeof(uint)) return SchemaPrimitiveType.UINT32;
        if (type == typeof(long)) return SchemaPrimitiveType.INT64;
        if (type == typeof(ulong)) return SchemaPrimitiveType.UINT64;
        if (type == typeof(float)) return SchemaPrimitiveType.SINGLE;
        if (type == typeof(double)) return SchemaPrimitiveType.DOUBLE;

        return SchemaPrimitiveType.UNDEFINED;
      }
    }
  }
}