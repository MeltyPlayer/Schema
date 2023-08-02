using schema.binary.parser;
using schema.util.symbols;


namespace schema.binary.attributes {
  internal class WSizeOfMemberInBytesParser : IAttributeParser {
    public void ParseIntoMemberType(IBetterSymbol memberSymbol,
                                    ITypeInfo memberTypeInfo,
                                    IMemberType memberType) {
      var sizeOfAttribute =
          memberSymbol.GetAttribute<WSizeOfMemberInBytesAttribute>();
      if (sizeOfAttribute == null) {
        return;
      }

      AccessChainUtil.AssertAllNodesInTypeChainUntilTargetUseBinarySchema(
          memberSymbol,
          sizeOfAttribute.AccessChainToOtherMember);

      if (memberTypeInfo is IIntegerTypeInfo &&
          memberType is BinarySchemaStructureParser.PrimitiveMemberType
              primitiveMemberType) {
        primitiveMemberType.AccessChainToSizeOf =
            sizeOfAttribute.AccessChainToOtherMember;
      } else {
        memberSymbol.ReportDiagnostic(Rules.NotSupported);
      }
    }
  }
}