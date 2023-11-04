using schema.binary.parser;
using schema.util.symbols;


namespace schema.binary.attributes {
  internal class WPointerToParser : IAttributeParser {
    public void ParseIntoMemberType(IBetterSymbol memberBetterSymbol,
                                    ITypeInfo memberTypeInfo,
                                    IMemberType memberType) {
      var pointerToAttribute =
          (IPointerToAttribute?) memberBetterSymbol
              .GetAttribute<WPointerToAttribute>() ??
          memberBetterSymbol.GetAttribute<WPointerToOrNullAttribute>();
      if (pointerToAttribute == null) {
        return;
      }

      AccessChainUtil.AssertAllNodesInTypeChainUntilTargetUseBinarySchema(
          memberBetterSymbol,
          pointerToAttribute.AccessChainToOtherMember);

      if (memberTypeInfo is IIntegerTypeInfo &&
          memberType is BinarySchemaContainerParser.PrimitiveMemberType
              primitiveMemberType) {
        primitiveMemberType.PointerToAttribute = pointerToAttribute;
      } else {
        memberBetterSymbol.ReportDiagnostic(Rules.NotSupported);
      }
    }
  }
}