using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;

using schema.binary.parser;
using schema.util.diagnostics;


namespace schema.binary.attributes {
  internal class SequenceLengthSourceParser : IAttributeParser {
    public void ParseIntoMemberType(IDiagnosticReporter diagnosticReporter,
                                    ISymbol memberSymbol,
                                    ITypeInfo memberTypeInfo,
                                    IMemberType memberType) {
      var lengthSourceAttribute =
          (ISequenceLengthSourceAttribute?) memberSymbol
              .GetAttribute<SequenceLengthSourceAttribute>(
                  diagnosticReporter) ?? memberSymbol
              .GetAttribute<RSequenceLengthSourceAttribute>(diagnosticReporter);
      var untilEndOfStreamAttribute =
          memberSymbol.GetAttribute<RSequenceUntilEndOfStreamAttribute>(
              diagnosticReporter);

      if (memberType is BinarySchemaStructureParser.SequenceMemberType
          sequenceMemberType) {
        if (sequenceMemberType.LengthSourceType ==
            SequenceLengthSourceType.UNSPECIFIED) {
          if (lengthSourceAttribute != null) {
            sequenceMemberType.LengthSourceType =
                lengthSourceAttribute.Method;

            switch (sequenceMemberType.LengthSourceType) {
              case SequenceLengthSourceType.IMMEDIATE_VALUE: {
                sequenceMemberType.ImmediateLengthType =
                    lengthSourceAttribute.LengthType;
                break;
              }
              case SequenceLengthSourceType.OTHER_MEMBER: {
                sequenceMemberType.LengthMember =
                    MemberReferenceUtil.WrapMemberReference(
                        lengthSourceAttribute.OtherMember);
                break;
              }
              case SequenceLengthSourceType.CONST_LENGTH: {
                sequenceMemberType.ConstLength =
                    lengthSourceAttribute.ConstLength;
                break;
              }
              default:
                throw new NotImplementedException();
            }
          } else if (untilEndOfStreamAttribute != null) {
            sequenceMemberType.LengthSourceType =
                SequenceLengthSourceType.UNTIL_END_OF_STREAM;
          } else {
            diagnosticReporter.ReportDiagnostic(
                Rules.MutableArrayNeedsLengthSource);
          }
        }
        // Didn't expect attribute b/c length is already specified
        else if (lengthSourceAttribute != null) {
          diagnosticReporter.ReportDiagnostic(
              Rules.UnexpectedAttribute);
        }
      }

      // Didn't expect attribute b/c not a sequence
      else if (lengthSourceAttribute != null) {
        diagnosticReporter.ReportDiagnostic(
            Rules.UnexpectedSequenceAttribute);
      }
    }
  }
}