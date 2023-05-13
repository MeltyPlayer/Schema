namespace schema.binary.attributes.child_of {
  /// <summary>
  ///   Schema interface for allowing a child to access the data of a specific parent class.
  ///
  ///   <para>
  ///     This attribute designates that a child type will and must be contained in the parent class.
  ///     When the parent type is being read and it reaches the child's field, the parent will pass itself into [child].Parent.///
  ///   </para>
  ///
  ///   <para>
  ///     This can be used alongside [Ignore] to easily reference values from the parent class:
  ///   </para>
  ///   <code>
  ///     [BinarySchema]
  ///     public partial class ParentType : IBinaryConvertible {
  ///       [NumberFormat(SchemaIntegerType.BYTE)]
  ///       public bool ChildHasSomeField { get; set; }
  ///
  ///       public uint BaseOffset { get; set; }
  /// 
  ///       public ChildType Child1 { get; } = new();
  ///
  ///       public ChildType Child2 { get; } = new();
  ///
  ///       public ChildType Child3 { get; } = new();
  ///     }
  /// 
  ///     [BinarySchema]
  ///     public partial class ChildType : IBinaryConvertible, IChildOf&lt;ParentType&gt; {
  ///       public ParentType Parent { get; set; }
  ///
  ///       [Ignore]
  ///       private bool HasSomeField => Parent.ChildHasSomeField;
  /// 
  ///       [IfBoolean(nameof(HasSomeField))]
  ///       public int? SomeField { get; set; }
  ///
  ///       [Ignore]
  ///       private bool BaseOffset => Parent.BaseOffset;
  ///
  ///       public uint ChildOffset { get; set; }
  ///
  ///       [Offset(nameof(BaseOffset), nameof(ChildOffset))]
  ///       public int SomeData { get; set; }
  ///     }
  ///   </code>
  /// </summary>
  public interface IChildOf<TParent> where TParent : IBinaryConvertible {
    public TParent Parent { get; set; }
  }
}