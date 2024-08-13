using System.Linq;

using Microsoft.CodeAnalysis;

using schema.binary.attributes;
using schema.util.asserts;
using schema.util.symbols;


namespace schema.binary;

public static class BinarySchemaSymbolUtil {
  public static bool IsAtLeastAsBinaryConvertibleAs(this ISymbol symbol,
                                                    ITypeSymbol other)
    => (!other.IsBinaryDeserializable() || symbol.IsBinaryDeserializable()) &&
       (!other.IsBinarySerializable() || symbol.IsBinarySerializable());

  public static bool IsBinarySerializable(this ISymbol symbol)
    => symbol.Implements<IBinarySerializable>();

  public static bool IsBinaryDeserializable(this ISymbol symbol)
    => symbol.Implements<IBinaryDeserializable>();

  public static bool IsChild(this ISymbol symbol, out INamedTypeSymbol parent) {
    if (symbol.Implements(typeof(IChildOf<>), out var matchingType)) {
      parent = Asserts.AsA<INamedTypeSymbol>(
          matchingType.TypeArguments.First());
      return true;
    }

    parent = default;
    return false;
  }

  public static bool IsIndexed(this ISymbol symbol)
    => symbol.Implements<IIndexed>();
}