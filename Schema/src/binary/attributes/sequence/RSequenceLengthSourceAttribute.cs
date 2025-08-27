using System;

using schema.util.diagnostics;


namespace schema.binary.attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class RSequenceLengthSourceAttribute
    : BMemberAttribute,
      ISequenceLengthSourceAttribute {
  private string? otherMemberName_;

  /// <summary>
  ///   Uses another integer field for the length. This separate field will
  ///   only be used when reading.
  /// </summary>
  public RSequenceLengthSourceAttribute(string otherMemberName) {
    this.Method = SequenceLengthSourceType.OTHER_MEMBER;
    this.otherMemberName_ = otherMemberName;
  }

  protected override void InitFields(IDiagnosticReporter diagnosticReporter,
                                     IMemberReference memberThisIsAttachedTo) {
    if (this.otherMemberName_ != null) {
      this.OtherMember =
          this.GetReadTimeOnlySourceRelativeToContainer(this.otherMemberName_);

      if (!memberThisIsAttachedTo.IsSequence) {
        diagnosticReporter.ReportDiagnostic(
            memberThisIsAttachedTo.MemberSymbol,
            Rules.SequenceLengthSourceCanOnlyBeUsedOnSequences);
      }
      if (!this.OtherMember.IsInteger) {
        diagnosticReporter.ReportDiagnostic(
            memberThisIsAttachedTo.MemberSymbol,
            Rules.RSequenceLengthSourceOtherFieldMustBeAnInteger);
      }
    }
  }

  public SequenceLengthSourceType Method { get; }

  public SchemaIntegerType LengthType { get; }
  public IMemberReference OtherMember { get; private set; }
  public uint ConstLength { get; }
}