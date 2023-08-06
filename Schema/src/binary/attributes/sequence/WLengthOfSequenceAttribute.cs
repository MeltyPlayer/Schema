using System;

namespace schema.binary.attributes {
  /// <summary>
  ///   Attribute for specifying that an integer represents the length of some
  ///   sequence.
  ///
  ///   <para>
  ///     Used at write-time to substitute that length in instead of the raw
  ///     value of this field.
  ///   </para>
  ///   <para>
  ///     Multiple of this attribute can be used on a single member, marking
  ///     that this represents the lengths of multiple other members. This will
  ///     result in extra write-time validation ensuring that their lengths are
  ///     equal.
  ///   </para>
  ///   <para>
  ///     If included within an IBinaryDeserializable, this will result in a
  ///     compile-time error since this is only used at write-time. If included
  ///     within an IBinaryConvertible, it will be enforced that any other members
  ///     that this is marked as a length of must have this marked as their
  ///     length source.
  ///   </para>
  /// </summary>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property,
                  AllowMultiple = true)]
  public class WLengthOfSequenceAttribute : BMemberAttribute {
    private string otherMemberName_;

    public WLengthOfSequenceAttribute(string otherMemberName) {
      this.otherMemberName_ = otherMemberName;
    }

    protected override void InitFields() {
      this.OtherMember =
          this.GetMemberRelativeToContainer(this.otherMemberName_)
              .AssertIsSequence();
    }

    public IMemberReference? OtherMember { get; private set; }
  }
}