using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

using schema.binary;

namespace schema.util.types {
  public interface ITypeV2 {
    string Name { get; }

    string? FullyQualifiedNamespace { get; }
    IEnumerable<string> NamespaceParts { get; }

    IEnumerable<string> DeclaringTypeNamesDownward { get; }

    bool IsExactly(ITypeV2 other);
    bool IsExactly<T>();
    bool IsExactly(Type other);
    bool IsExactly(ISymbol other);

    bool Implements<T>();
    bool Implements(Type type);

    bool Implements<T>(out ITypeV2 matchingType);
    bool Implements(Type type, out ITypeV2 matchingType);

    int Arity { get; }
    bool HasGenericArguments(out IEnumerable<ITypeV2> genericArguments);
    IEnumerable<ITypeV2> GenericArguments { get; }
    bool HasGenericConstraints(out IEnumerable<ITypeV2> genericConstraints);
    IEnumerable<ITypeV2> GenericConstraints { get; }

    bool HasNullableAnnotation { get; }
    bool IsAtLeastAsBinaryConvertibleAs(ITypeV2 other);
    bool IsBinarySerializable { get; }
    bool IsBinaryDeserializable { get; }
    bool IsClass { get; }
    bool IsInterface { get; }
    bool IsStruct { get; }
    bool IsString { get; }
    bool IsArray(out ITypeV2 elementType);
    bool IsPrimitive(out SchemaPrimitiveType primitiveType);
    bool IsEnum(out SchemaIntegerType underlyingType);

    bool IsSequence(out ITypeV2 elementType,
                    out SequenceType sequenceType);

    bool IsChild(out ITypeV2 parent);
    bool ContainsMemberWithType(ITypeV2 other);

    bool HasAttribute<TAttribute>() where TAttribute : Attribute;
    TAttribute GetAttribute<TAttribute>() where TAttribute : Attribute;

    IEnumerable<TAttribute> GetAttributes<TAttribute>()
        where TAttribute : Attribute;
  }
}