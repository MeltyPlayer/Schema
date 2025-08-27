using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;

using schema.binary.attributes;
using schema.util.asserts;


namespace schema.util.symbols;

internal static partial class BetterSymbol {
  private partial class BetterSymbolImpl {
    private ImmutableArray<Attribute> attributes_;

    private void InitAttributes() {
      var attributeData
          = this.Symbol.GetAttributes()
                .SkipWhile(a => !a.AttributeClass?.GetFullyQualifiedNamespace()
                                  ?.StartsWith("schema.") ??
                                true);

      this.attributes_
          = attributeData
            .Select(attributeData => {
                      var attributeType
                          = Asserts.CastNonnull(
                              attributeData.AttributeClass?.LookUpType());

                      var attribute = attributeData.Instantiate(
                          attributeType,
                          this.Symbol);
                      if (attribute is BMemberAttribute
                          memberAttribute) {
                        memberAttribute.Init(this,
                                             this.Symbol.ContainingType,
                                             this.Symbol.Name);
                      }

                      return attribute;
                    })
            .ToImmutableArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasAttribute<TAttribute>() where TAttribute : Attribute
      => this.GetAttributes<TAttribute>().Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TAttribute? GetAttribute<TAttribute>()
        where TAttribute : Attribute
      => this.GetAttributes<TAttribute>()
             .SingleOrDefault();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TAttribute> GetAttributes<TAttribute>()
        where TAttribute : Attribute
      => this.attributes_.OfType<TAttribute>();
  }
}