using Microsoft.CodeAnalysis;

using schema.binary.attributes;


namespace schema.binary;

public static partial class Rules {
  public static readonly DiagnosticDescriptor
      SequenceLengthSourceCanOnlyBeUsedOnSequences
          = Rules.CreateDiagnosticDescriptor_(
              $"{nameof(SequenceLengthSourceAttribute)} can only be used on sequences",
              $"Field '{{0}}' is not a sequence, so {nameof(SequenceLengthSourceAttribute)} cannot be used on it.");

  public static readonly DiagnosticDescriptor
      RSequenceLengthSourceOtherFieldMustBeAnInteger
          = Rules.CreateDiagnosticDescriptor_(
              $"The field {nameof(RSequenceLengthSourceAttribute)} references for length must be an integer.",
              "The other field that '{0}' is using for its length is not an integer.");

  public static readonly DiagnosticDescriptor
      StringLengthSourceCanOnlyBeUsedOnSequences
          = Rules.CreateDiagnosticDescriptor_(
              $"{nameof(StringLengthSourceAttribute)} can only be used on strings",
              $"Field '{{0}}' is not a string, so {nameof(StringLengthSourceAttribute)} cannot be used on it.");

  public static readonly DiagnosticDescriptor
      RStringLengthSourceOtherFieldMustBeAnInteger
          = Rules.CreateDiagnosticDescriptor_(
              $"The field {nameof(RStringLengthSourceAttribute)} references for length must be an integer.",
              "The other field that '{0}' is using for its length is not an integer.");

  public static readonly DiagnosticDescriptor
      FixedPointCanOnlyBeUsedOnFloats
          = Rules.CreateDiagnosticDescriptor_(
              $"{nameof(FixedPointAttribute)} can only be used on floats",
              $"Field '{{0}}' is not a float, so {nameof(FixedPointAttribute)}  cannot be used on it.");

  public static readonly DiagnosticDescriptor
      NullTerminatedStringCanOnlyBeUsedOnStrings
          = Rules.CreateDiagnosticDescriptor_(
              $"{nameof(NullTerminatedStringAttribute)} can only be used on strings",
              $"Field '{{0}}' is not a string, so {nameof(NullTerminatedStringAttribute)}  cannot be used on it.");

  public static readonly DiagnosticDescriptor
      StringEncodingCanOnlyBeUsedOnStrings
          = Rules.CreateDiagnosticDescriptor_(
              $"{nameof(StringEncodingAttribute)} can only be used on strings",
              $"Field '{{0}}' is not a string, so {nameof(StringEncodingAttribute)}  cannot be used on it.");
}