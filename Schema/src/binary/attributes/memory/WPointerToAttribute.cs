using System;


namespace schema.binary.attributes.memory {
  /// <summary>
  ///   Pointer that encodes the relative difference between some address and
  ///   the start of the containing stream.
  /// </summary>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class WPointerToAttribute : BMemberAttribute {
    private readonly string otherMemberName_;

    public WPointerToAttribute(string otherMemberName) {
      this.otherMemberName_ = otherMemberName;
    }

    protected override void InitFields() {
      this.AccessChainToOtherMember =
          this.GetAccessChainRelativeToStructure(
              this.otherMemberName_, false);
    }

    public IChain<IAccessChainNode> AccessChainToOtherMember {
      get;
      private set;
    }
  }
}