﻿using System;
using System.Collections.Generic;
using System.Linq;

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

      public abstract int GenericArgCount { get; }

      public abstract bool IsClass { get; }
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
           this.GenericArgCount == genericArgCount;

      public bool IsExactly(ITypeV2 other) => this.Matches_(
          other.Name,
          other.FullyQualifiedNamespace,
          other.GenericArgCount);

      public bool IsExactly(Type other) => this.Matches_(
          other.Name,
          other.Namespace,
          other.GenericTypeArguments.Length);

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

      public IEnumerable<ITypeV2> GenericConstraints
        => this.HasGenericConstraints(out var genericConstraints)
            ? genericConstraints
            : Enumerable.Empty<ITypeV2>();
    }
  }
}