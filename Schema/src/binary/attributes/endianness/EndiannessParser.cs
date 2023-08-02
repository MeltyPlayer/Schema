using System.IO;

using schema.util.symbols;


namespace schema.binary.attributes {
  internal class EndiannessParser {
    public Endianness? GetEndianness(IBetterSymbol betterSymbol)
      => betterSymbol.GetAttribute<EndiannessAttribute>()?.Endianness;
  }
}