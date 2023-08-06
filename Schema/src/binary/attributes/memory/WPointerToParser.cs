using schema.binary.parser;
using schema.util.symbols;


namespace schema.binary.attributes {
  internal class WPointerToParser : IAttributeParser {
    public void ParseIntoMemberType(IBetterSymbol memberBetterSymbol,
                                    ITypeInfo memberTypeInfo,
                                    IMemberType memberType) {
      var pointerToAttribute = memberBetterSymbol.GetAttribute<WPointerToAttribute>();
      if (pointerToAttribute == null) {
        return;
      }

      AccessChainUtil.AssertAllNodesInTypeChainUntilTargetUseBinarySchema(
          memberBetterSymbol,
          pointerToAttribute.AccessChainToOtherMember);

      if (memberTypeInfo is IIntegerTypeInfo &&
          memberType is BinarySchemaContainerParser.PrimitiveMemberType
              primitiveMemberType) {
        primitiveMemberType.AccessChainToPointer =
            pointerToAttribute.AccessChainToOtherMember;
      } else {
        memberBetterSymbol.ReportDiagnostic(Rules.NotSupported);
      }
    }
  }
}