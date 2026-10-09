using Microsoft.CodeAnalysis;

using schema.binary.attributes;


namespace schema.binary;

public static partial class Rules {
  public static readonly DiagnosticDescriptor
      SequenceLengthSourceCanOnlyBeUsedOnSequences
          = Rules.CreateDiagnosticDescriptor(
              $"{nameof(SequenceLengthSourceAttribute)} can only be used on sequences",
              $"Field '{{0}}' is not a sequence, so {nameof(SequenceLengthSourceAttribute)} cannot be used on it.");

  public static readonly DiagnosticDescriptor
      RSequenceLengthSourceOtherFieldMustBeAnInteger
          = Rules.CreateDiagnosticDescriptor(
              $"The field {nameof(RSequenceLengthSourceAttribute)} references for length must be an integer.",
              "The other field that '{0}' is using for its length is not an integer.");

  public static readonly DiagnosticDescriptor
      RStringLengthSourceOtherFieldMustBeAnInteger
          = Rules.CreateDiagnosticDescriptor(
              $"The field {nameof(RStringLengthSourceAttribute)} references for length must be an integer.",
              "The other field that '{0}' is using for its length is not an integer.");
}