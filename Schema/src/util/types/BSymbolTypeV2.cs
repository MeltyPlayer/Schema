using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

using schema.binary;
using schema.util.symbols;

namespace schema.util.types {
  public static partial class TypeV2 {
    private abstract class BSymbolTypeV2 : ITypeV2 {
      public abstract string Name { get; }

      public abstract string? FullyQualifiedNamespace { get; }
      public abstract IEnumerable<string> NamespaceParts { get; }

      public abstract bool Implements(Type type);

      public abstract int GenericArgCount { get; }

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

      public bool IsBinarySerializable
        => this.Implements<IBinarySerializable>();

      public bool IsBinaryDeserializable
        => this.Implements<IBinaryDeserializable>();
    }
  }
}