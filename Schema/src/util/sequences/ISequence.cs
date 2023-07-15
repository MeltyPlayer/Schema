using System.Collections.Generic;

namespace schema.util.sequences {
  public interface IReadOnlySequence<out T> : IEnumerable<T> {
    int Count { get; }
    T this[int index] { get; }

    IReadOnlySequence<T> CloneWithNewLength(int newLength);
  }

  public interface ISequence<T> : IReadOnlySequence<T> {
    new int Count { get; set; }
    new T this[int index] { get; set; }
    void Clear();
  }
}
