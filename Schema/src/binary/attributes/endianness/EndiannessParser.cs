using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;


namespace schema.binary.attributes {
  public class EndiannessParser {
    public Endianness? GetEndianness(
        IList<Diagnostic> diagnostics,
        ISymbol symbol) {
      var endiannessAttribute =
          SymbolTypeUtil.GetAttribute<EndiannessAttribute>(diagnostics, symbol);
      return endiannessAttribute?.Endianness;
    }
  }
}