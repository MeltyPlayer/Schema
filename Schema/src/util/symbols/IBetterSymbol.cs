using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

using schema.util.diagnostics;


namespace schema.util.symbols;

public interface IBetterSymbol : IDiagnosticReporter {
  ISymbol Symbol { get; }
  string Name { get; }

  IBetterSymbol GetChild(ISymbol child);

  // Attributes
  bool HasAttribute<TAttribute>() where TAttribute : Attribute;
  TAttribute? GetAttribute<TAttribute>() where TAttribute : Attribute;

  IEnumerable<TAttribute> GetAttributes<TAttribute>()
      where TAttribute : Attribute;
}

public interface IBetterSymbol<out TSymbol> : IBetterSymbol
    where TSymbol : ISymbol {
  TSymbol TypedSymbol { get; }
}