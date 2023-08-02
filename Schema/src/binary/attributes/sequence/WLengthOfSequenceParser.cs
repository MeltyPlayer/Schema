using Microsoft.CodeAnalysis;

using System.Collections.Generic;
using System.Linq;

using schema.binary.parser;
using schema.util.diagnostics;


namespace schema.binary.attributes {
  internal class WLengthOfSequenceParser : IAttributeParser {
    public void ParseIntoMemberType(IDiagnosticReporter diagnosticReporter,
                                    ISymbol memberSymbol,
                                    ITypeInfo memberTypeInfo,
                                    IMemberType memberType) {
      var lengthOfSequenceAttributes =
          memberSymbol
              .GetAttributes<WLengthOfSequenceAttribute>(diagnosticReporter)
              .ToArray();
      if (lengthOfSequenceAttributes.Length == 0) {
        return;
      }

      if (memberTypeInfo is IIntegerTypeInfo &&
          memberType is BinarySchemaStructureParser.PrimitiveMemberType
              primitiveMemberType) {
        primitiveMemberType.LengthOfSequenceMembers =
            lengthOfSequenceAttributes.Select(attr => attr.OtherMember)
                                      .ToArray();
      } else {
        diagnosticReporter.ReportDiagnostic(Rules.NotSupported);
      }
    }
  }
}