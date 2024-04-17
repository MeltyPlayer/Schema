using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;

using schema.binary;
using schema.binary.attributes;
using schema.util.sequences;
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

      public abstract bool IsGenericTypeParameter(
          out IEnumerable<ITypeV2> genericConstraints);

      public abstract IEnumerable<(string, ITypeV2)> GetTupleElements();


      public abstract bool ContainsMemberWithType(ITypeV2 other);

      public abstract bool HasAttribute<TAttribute>()
          where TAttribute : Attribute;

      public abstract TAttribute GetAttribute<TAttribute>()
          where TAttribute : Attribute;

      public abstract IEnumerable<TAttribute> GetAttributes<TAttribute>()
          where TAttribute : Attribute;


      // Common
      public string FullyQualifiedName {
        get {
          var namespacePortion = this.FullyQualifiedNamespace ?? "";
          if (namespacePortion.Length > 0) {
            namespacePortion += ".";
          }

          var declaringTypesPortion = "";
          var declaringTypes = this.DeclaringTypeNamesDownward.ToArray();
          if (declaringTypes.Length > 0) {
            declaringTypesPortion = $"{string.Join(".", declaringTypes)}.";
          }

          var name = this.Name;

          return $"{namespacePortion}{declaringTypesPortion}{name}";
        }
      }

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
          expectedArity =
              int.Parse(expectedName.Substring(indexOfBacktick + 1));
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

      public bool IsAtLeastAsBinaryConvertibleAs(ITypeV2 other)
        => (!other.IsBinaryDeserializable || this.IsBinaryDeserializable) &&
           (!other.IsBinarySerializable || this.IsBinarySerializable);

      public bool IsBinarySerializable
        => this.Implements<IBinarySerializable>();

      public bool IsBinaryDeserializable
        => this.Implements<IBinaryDeserializable>();

      public bool IsChild(out ITypeV2 parent) {
        if (this.Implements(typeof(IChildOf<>), out var matchingType)) {
          parent = matchingType.GenericArguments.First();
          return true;
        }

        parent = default;
        return false;
      }

      public IEnumerable<ITypeV2> GenericArguments
        => this.HasGenericArguments(out var genericArguments)
            ? genericArguments
            : Enumerable.Empty<ITypeV2>();

      public IEnumerable<ITypeV2> GenericConstraints
        => this.IsGenericTypeParameter(out var genericConstraints)
            ? genericConstraints
            : Enumerable.Empty<ITypeV2>();

      public bool IsSequence(out ITypeV2 elementType,
                             out SequenceType sequenceType) {
        if (this.IsArray(out elementType)) {
          sequenceType = SequenceType.MUTABLE_ARRAY;
          return true;
        }

        if (this.Implements(typeof(ImmutableArray<>),
                            out var immutableArrayTypeV2)) {
          elementType = immutableArrayTypeV2.GenericArguments.ToArray()[0];
          sequenceType = SequenceType.IMMUTABLE_ARRAY;
          return true;
        }

        if (this.Implements(typeof(ISequence<,>), out var sequenceTypeV2)) {
          elementType = sequenceTypeV2.GenericArguments.ToArray()[1];
          sequenceType = SequenceType.MUTABLE_SEQUENCE;
          return true;
        }

        if (this.Implements(typeof(IConstLengthSequence<,>),
                            out var constLengthSequenceTypeV2)) {
          elementType = constLengthSequenceTypeV2.GenericArguments.ToArray()[1];
          sequenceType = SequenceType.MUTABLE_SEQUENCE;
          return true;
        }

        if (this.Implements(typeof(IReadOnlySequence<,>),
                            out var readOnlySequence)) {
          elementType = readOnlySequence.GenericArguments.ToArray()[1];
          sequenceType = SequenceType.READ_ONLY_SEQUENCE;
          return true;
        }

        if (this.Implements(typeof(List<>), out var listTypeV2)) {
          elementType = listTypeV2.GenericArguments.ToArray()[0];
          sequenceType = SequenceType.MUTABLE_LIST;
          return true;
        }

        if (this.Implements(typeof(IReadOnlyList<>),
                            out var readonlyListTypeV2)) {
          elementType = readonlyListTypeV2.GenericArguments.ToArray()[0];
          sequenceType = SequenceType.READ_ONLY_LIST;
          return true;
        }

        elementType = default;
        sequenceType = default;
        return false;
      }
    }
  }
}