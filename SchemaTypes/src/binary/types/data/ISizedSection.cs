namespace schema.binary.types.data {
  public interface ISizedSection<T> : IBinaryConvertible
      where T : IBinaryConvertible {
    T Data { get; }
  }
}