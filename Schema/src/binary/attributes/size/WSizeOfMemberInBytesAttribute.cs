using System;


namespace schema.binary.attributes.size {
  /// <summary>
  ///   Attribute for specifying that an integer represents the size of some
  ///   structure.
  ///
  ///   <para>
  ///     Used at write-time to substitute that length in instead of the raw
  ///     value of this field.
  ///   </para>
  ///   <para>
  ///     If included within an IBinaryDeserializable, this will result in a
  ///     compile-time error since this is only used at write-time.
  ///   </para>
  /// </summary>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class WSizeOfMemberInBytesAttribute : BMemberAttribute {
    private string otherMemberName_;

    public WSizeOfMemberInBytesAttribute(string otherMemberName) {
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