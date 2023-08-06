using schema.binary.parser;
using schema.util.symbols;


namespace schema.binary.attributes {
  internal class WSizeOfMemberInBytesParser : IAttributeParser {
    public void ParseIntoMemberType(IBetterSymbol memberBetterSymbol,
                                    ITypeInfo memberTypeInfo,
                                    IMemberType memberType) {
      var sizeOfAttribute =
          memberBetterSymbol.GetAttribute<WSizeOfMemberInBytesAttribute>();
      if (sizeOfAttribute == null) {
        return;
      }

      AccessChainUtil.AssertAllNodesInTypeChainUntilTargetUseBinarySchema(
          memberBetterSymbol,
          sizeOfAttribute.AccessChainToOtherMember);

      if (memberTypeInfo is IIntegerTypeInfo &&
          memberType is BinarySchemaContainerParser.PrimitiveMemberType
              primitiveMemberType) {
        primitiveMemberType.AccessChainToSizeOf =
            sizeOfAttribute.AccessChainToOtherMember;
      } else {
        memberBetterSymbol.ReportDiagnostic(Rules.NotSupported);
      }
    }
  }
}