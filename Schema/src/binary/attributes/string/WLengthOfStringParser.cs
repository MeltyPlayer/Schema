using Microsoft.CodeAnalysis;

using System.Collections.Generic;
using System.Linq;

using schema.binary.parser;
using schema.util.diagnostics;


namespace schema.binary.attributes {
  internal class WLengthOfStringParser : IAttributeParser {
    public void ParseIntoMemberType(IDiagnosticReporter diagnosticReporter,
                                    ISymbol memberSymbol,
                                    ITypeInfo memberTypeInfo,
                                    IMemberType memberType) {
      var lengthOfStringAttributes =
          memberSymbol
              .GetAttributes<WLengthOfStringAttribute>(diagnosticReporter)
              .ToArray();
      if (lengthOfStringAttributes.Length == 0) {
        return;
      }

      if (memberTypeInfo is IIntegerTypeInfo &&
          memberType is BinarySchemaStructureParser.PrimitiveMemberType
              primitiveMemberType) {
        primitiveMemberType.LengthOfStringMembers =
            lengthOfStringAttributes.Select(attr => attr.OtherMember)
                                    .ToArray();
      } else {
        diagnosticReporter.ReportDiagnostic(Rules.NotSupported);
      }
    }
  }
}