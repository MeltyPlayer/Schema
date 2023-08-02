using schema.binary.parser;
using schema.util.symbols;


namespace schema.binary.attributes {
  internal class WPointerToParser : IAttributeParser {
    public void ParseIntoMemberType(IBetterSymbol memberSymbol,
                                    ITypeInfo memberTypeInfo,
                                    IMemberType memberType) {
      var pointerToAttribute = memberSymbol.GetAttribute<WPointerToAttribute>();
      if (pointerToAttribute == null) {
        return;
      }

      AccessChainUtil.AssertAllNodesInTypeChainUntilTargetUseBinarySchema(
          memberSymbol,
          pointerToAttribute.AccessChainToOtherMember);

      if (memberTypeInfo is IIntegerTypeInfo &&
          memberType is BinarySchemaStructureParser.PrimitiveMemberType
              primitiveMemberType) {
        primitiveMemberType.AccessChainToPointer =
            pointerToAttribute.AccessChainToOtherMember;
      } else {
        memberSymbol.ReportDiagnostic(Rules.NotSupported);
      }
    }
  }
}