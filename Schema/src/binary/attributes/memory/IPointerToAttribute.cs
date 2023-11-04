namespace schema.binary.attributes {
  public interface IPointerToAttribute {
    IChain<IAccessChainNode> AccessChainToOtherMember { get; }
    int? NullValue { get; }
  }
}