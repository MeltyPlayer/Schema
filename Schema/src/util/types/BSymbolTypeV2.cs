using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

using Microsoft.CodeAnalysis;

using schema.binary;
using schema.util.symbols;

namespace schema.util.types {
  public static partial class TypeV2 {
    private abstract class BSymbolTypeV2 : ITypeV2 {
      public abstract string Name { get; }

      public abstract string? FullyQualifiedNamespace { get; }
      public abstract IEnumerable<string> NamespaceParts { get; }
      public abstract IEnumerable<string> DeclaringTypeNamesDownward { get; }

      public abstract bool Implements(Type type, out ITypeV2 matchingType);

      public abstract int Arity { get; }

      public virtual bool HasNullableAnnotation => false;
      public abstract bool IsClass { get; }
      public abstract bool IsInterface { get; }
      public abstract bool IsStruct { get; }
      public abstract bool IsString { get; }
      public abstract bool IsArray(out ITypeV2 elementType);
      public abstract bool IsPrimitive(out SchemaPrimitiveType primitiveType);
      public abstract bool IsEnum(out SchemaIntegerType underlyingType);

      public abstract bool HasGenericArguments(
          out IEnumerable<ITypeV2> genericArguments);

      public abstract bool HasGenericConstraints(
          out IEnumerable<ITypeV2> genericConstraints);

      public abstract bool HasAttribute<TAttribute>()
          where TAttribute : Attribute;

      public abstract TAttribute GetAttribute<TAttribute>()
          where TAttribute : Attribute;

      public abstract IEnumerable<TAttribute> GetAttributes<TAttribute>()
          where TAttribute : Attribute;


      // Common
      private bool Matches_(string name,
                            string? fullyQualifiedNamespace,
                            int genericArgCount)
        => this.Name == name &&
           this.FullyQualifiedNamespace == fullyQualifiedNamespace &&
           this.Arity == genericArgCount;

      public bool IsExactly(ITypeV2 other) => this.Matches_(
          other.Name,
          other.FullyQualifiedNamespace,
          other.Arity);

      public bool IsExactly(Type other) {
        var expectedName = other.Name;

        int expectedArity = 0;
        var indexOfBacktick = expectedName.IndexOf('`');
        if (indexOfBacktick != -1) {
          expectedArity = int.Parse(expectedName.Substring(indexOfBacktick + 1));
          expectedName = expectedName.Substring(0, indexOfBacktick);
        }

        return this.Matches_(
            expectedName,
            other.Namespace,
            expectedArity);
      }

      public bool IsExactly(ISymbol other) => this.Matches_(
          other.Name,
          other.GetFullyQualifiedNamespace(),
          (other as INamedTypeSymbol)?.TypeParameters.Length ?? 0);

      public bool IsExactly<T>() => this.IsExactly(typeof(T));

      public bool Implements<T>() => this.Implements(typeof(T));

      public bool Implements<T>(out ITypeV2 matchingType)
        => this.Implements(typeof(T), out matchingType);

      public bool Implements(Type type) => this.Implements(type, out _);

      public bool IsBinarySerializable
        => this.Implements<IBinarySerializable>();

      public bool IsBinaryDeserializable
        => this.Implements<IBinaryDeserializable>();

      public IEnumerable<ITypeV2> GenericArguments
        => this.HasGenericArguments(out var genericArguments)
            ? genericArguments
            : Enumerable.Empty<ITypeV2>();

      public IEnumerable<ITypeV2> GenericConstraints
        => this.HasGenericConstraints(out var genericConstraints)
            ? genericConstraints
            : Enumerable.Empty<ITypeV2>();

      public virtual ITypeSymbol TypeSymbol
        => throw new NotImplementedException();
    }
  }
}