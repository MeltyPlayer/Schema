using Microsoft.CodeAnalysis;
using System.Collections.Generic;


namespace schema.binary.attributes.align {
  public class AlignAttributeParser {
    public int GetAlignForMember(
        IList<Diagnostic> diagnostics,
        ISymbol memberSymbol) {
      var alignAttribute =
          SymbolTypeUtil.GetAttribute<AlignAttribute>(
              diagnostics, memberSymbol);
      return alignAttribute?.Align ?? 0;
    }
  }
}