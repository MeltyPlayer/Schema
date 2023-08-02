using Microsoft.CodeAnalysis;

using schema.binary.parser;
using schema.util.diagnostics;


namespace schema.binary.attributes {
  internal class WSizeOfMemberInBytesParser : IAttributeParser {
    public void ParseIntoMemberType(IDiagnosticReporter diagnosticReporter,
                                    ISymbol memberSymbol,
                                    ITypeInfo memberTypeInfo,
                                    IMemberType memberType) {
      var sizeOfAttribute =
          memberSymbol.GetAttribute<WSizeOfMemberInBytesAttribute>(
              diagnosticReporter);
      if (sizeOfAttribute == null) {
        return;
      }

      AccessChainUtil.AssertAllNodesInTypeChainUntilTargetUseBinarySchema(
          diagnosticReporter,
          sizeOfAttribute.AccessChainToOtherMember);

      if (memberTypeInfo is IIntegerTypeInfo &&
          memberType is BinarySchemaStructureParser.PrimitiveMemberType
              primitiveMemberType) {
        primitiveMemberType.AccessChainToSizeOf =
            sizeOfAttribute.AccessChainToOtherMember;
      } else {
        diagnosticReporter.ReportDiagnostic(Rules.NotSupported);
      }
    }
  }
}