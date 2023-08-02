using Microsoft.CodeAnalysis;
using System.IO;

using schema.util.diagnostics;


namespace schema.binary.attributes {
  internal class EndiannessParser {
    public Endianness? GetEndianness(
        IDiagnosticReporter diagnosticReporter,
        ISymbol symbol) {
      var endiannessAttribute =
          symbol.GetAttribute<EndiannessAttribute>(diagnosticReporter);
      return endiannessAttribute?.Endianness;
    }
  }
}