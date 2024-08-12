using System;


namespace schema.binary.attributes {
  /// <summary>
  ///   Pointer that encodes the relative difference between some address and
  ///   the start of the containing stream. Null values
  /// </summary>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class WPointerToOrNullAttribute : BMemberAttribute,
                                           IPointerToAttribute {
    private readonly string otherMemberName_;

    public WPointerToOrNullAttribute(string otherMemberName,
                                     long nullValue = 0) {
      this.otherMemberName_ = otherMemberName;
      this.NullValue = nullValue;
    }

    protected override void InitFields() {
      this.AccessChainToOtherMember =
          this.GetAccessChainRelativeToContainer(
              this.otherMemberName_,
              false);
    }

    public IChain<IAccessChainNode> AccessChainToOtherMember {
      get;
      private set;
    }

    public long? NullValue { get; }
  }
}