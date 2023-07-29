﻿namespace schema.binary.types.data {
  public interface IMagicSection<T> : IMagicSection<string, T>
      where T : IBinaryConvertible { }

  public interface IMagicSection<out TMagic, TData> : ISizedSection<TData>
      where TData : IBinaryConvertible {
    TMagic Magic { get; }
  }
}