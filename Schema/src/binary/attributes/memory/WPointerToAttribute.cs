using System;

using schema.util.diagnostics;


namespace schema.binary.attributes;

/// <summary>
///   Pointer that encodes the relative difference between some address and
///   the start of the containing stream.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class WPointerToAttribute : BMemberAttribute, IPointerToAttribute {
  private readonly string otherMemberName_;

  public WPointerToAttribute(string otherMemberName) {
    this.otherMemberName_ = otherMemberName;
  }

  protected override void InitFields(IDiagnosticReporter diagnosticReporter,
                                     IMemberReference memberThisIsAttachedTo) {
    this.AccessChainToOtherMember =
        this.GetAccessChainRelativeToContainer(
            this.otherMemberName_,
            false);
  }

  public IChain<IAccessChainNode> AccessChainToOtherMember { get; private set; }

  public long? NullValue => null;
}