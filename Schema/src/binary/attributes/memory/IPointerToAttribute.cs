namespace schema.binary.attributes {
  public interface IPointerToAttribute {
    IChain<IAccessChainNode> AccessChainToOtherMember { get; }
    long? NullValue { get; }
  }
}