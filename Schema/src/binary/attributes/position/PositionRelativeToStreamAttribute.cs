using System;


namespace schema.binary.attributes.position {
  /// <summary>
  ///   Schema attribute for getting/asserting the current position of an EndianBinaryReader.
  ///   The type of a member with this attribute must be long, to correspond to the type of EndianBinaryReader.Position.
  ///
  ///   <para>
  ///     If the value is mutable, then it will be used to store the current position.
  ///   </para>
  ///   <para>
  ///     If the value is readonly, then it will be used to assert the current position instead.
  ///   </para>
  ///
  ///   <para>
  ///     For example, the following could be used to verify the size of a data structure is correct:
  ///   </para>
  ///   <code>
  ///     [BinarySchema]
  ///     public partial class SpecifiedSizeType : IBinaryConvertible {
  ///       [Position]
  ///       private long StartPosition_ { get; set; }
  ///
  ///       private long Length_ { get; set; }
  ///
  ///       ...
  /// 
  ///       [Position]
  ///       private uint ExpectedPosition_ => this.StartPosition_ + this.Length_;
  ///     }
  ///   </code>
  /// </summary>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class PositionRelativeToStreamAttribute : Attribute {
    protected void InitFields() { }
  }

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class LengthOfStreamAttribute : Attribute {
  }

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class LengthOfContainingStructureAttribute : Attribute {
  }
}