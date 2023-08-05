
using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace schema.util.types {
  public interface ITypeV2 {
    string Name { get; }

    string? FullyQualifiedNamespace { get; }
    IEnumerable<string> NamespaceParts { get; }

    bool IsExactly(ITypeV2 other);
    bool IsExactly<T>();
    bool IsExactly(Type other);
    bool IsExactly(ISymbol other);

    bool Implements<T>();
    bool Implements(Type type);

    bool IsBinarySerializable { get; }
    bool IsBinaryDeserializable { get; }

    int GenericArgCount { get; }
  }
}
