using System;
using System.Collections.Generic;
using System.Linq;

using schema.util.enumerables;

namespace schema.util.types {
  public static partial class TypeV2 {
    public static ITypeV2 FromType<T>() => FromType(typeof(T));

    public static ITypeV2 FromType(Type type)
      => new TypeTypeV2(type);

    private class TypeTypeV2 : BSymbolTypeV2 {
      private readonly Type type_;

      public TypeTypeV2(Type type) {
        this.type_ = type;
      }

      public override string Name => this.type_.Name;

      public override string? FullyQualifiedNamespace => this.type_.Namespace;

      public override IEnumerable<string> NamespaceParts
        => this.type_.Namespace?.Split('.') ?? Enumerable.Empty<string>();

      public override bool Implements(Type other)
        => this.type_.Yield()
               .Concat(this.type_.GetInterfaces())
               .Any(type => type == other);

      public override int GenericArgCount
        => this.type_.GetGenericParameterConstraints().Length;
    }
  }
}