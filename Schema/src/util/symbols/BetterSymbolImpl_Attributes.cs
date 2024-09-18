using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;

using schema.binary.attributes;


namespace schema.util.symbols;

internal static partial class BetterSymbol {
  private partial class BetterSymbolImpl {
    private ImmutableArray<AttributeData> attributeData_;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasAttribute<TAttribute>() where TAttribute : Attribute
      => this.GetAttributeData_<TAttribute>().Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TAttribute? GetAttribute<TAttribute>()
        where TAttribute : Attribute
      => this.GetAttributes<TAttribute>()
             .SingleOrDefault();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TAttribute> GetAttributes<TAttribute>()
        where TAttribute : Attribute
      => this.GetAttributeData_<TAttribute>()
             .Select(attributeData => {
                       var attribute =
                           attributeData.Instantiate<TAttribute>(this.Symbol);
                       if (attribute is BMemberAttribute memberAttribute) {
                         memberAttribute.Init(this,
                                              this.Symbol.ContainingType,
                                              this.Symbol.Name);
                       }

                       return attribute;
                     });


    private IEnumerable<AttributeData> GetAttributeData_<TAttribute>()
        where TAttribute : Attribute {
      var attributeType = typeof(TAttribute);
      return this.GetAttributeData_()
                 .Where(attributeData
                            => attributeData.AttributeClass?.IsType(
                                   attributeType) ??
                               false);
    }

    private ImmutableArray<AttributeData> GetAttributeData_() {
      if (attributeData_ != null) {
        return this.attributeData_;
      }

      return this.attributeData_ = this.Symbol.GetAttributes();
    }
  }
}