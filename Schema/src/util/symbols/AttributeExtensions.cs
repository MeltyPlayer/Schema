using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;

using schema.binary.attributes;
using schema.util.diagnostics;


namespace schema.util.symbols;

public static class AttributeExtensions {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static IEnumerable<AttributeData>
      GetAttributeData<TAttribute>(this ISymbol symbol) {
    var attributeType = typeof(TAttribute);
    return symbol
           .GetAttributes()
           .Where(attributeData
                      => attributeData.AttributeClass?.IsType(
                             attributeType) ??
                         false);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static bool HasAttribute<TAttribute>(this ISymbol symbol)
      where TAttribute : Attribute
    => symbol.GetAttributeData<TAttribute>().Any();


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static TAttribute? GetAttribute<TAttribute>(
      this ISymbol symbol,
      IDiagnosticReporter? diagnosticReporter)
      where TAttribute : Attribute
    => symbol.GetAttributes<TAttribute>(diagnosticReporter)
             .SingleOrDefault();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static IEnumerable<TAttribute> GetAttributes<TAttribute>(
      this ISymbol symbol,
      IDiagnosticReporter? diagnosticReporter = null)
      where TAttribute : Attribute
    => symbol.GetAttributeData<TAttribute>()
             .Select(attributeData => {
                       var attribute
                           = attributeData.Instantiate<TAttribute>(symbol);
                       if (attribute is BMemberAttribute memberAttribute) {
                         memberAttribute.Init(diagnosticReporter,
                                              symbol.ContainingType,
                                              symbol.Name);
                       }

                       return attribute;
                     });
}