using Microsoft.CodeAnalysis;

using System.Collections.Generic;


namespace schema.binary.attributes.align {
  public class AlignAttributeParser {
    public int GetAlignForMember(
        IList<Diagnostic> diagnostics,
        ISymbol memberSymbol)
      => memberSymbol.GetAttribute<AlignAttribute>(diagnostics)?.Align ?? 0;
  }
}