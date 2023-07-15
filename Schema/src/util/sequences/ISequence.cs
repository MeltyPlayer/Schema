using System.Collections.Generic;

namespace schema.util.sequences {
  public interface IReadOnlySequence<out T> : IEnumerable<T> {
    int Length { get; }
    T this[int index] { get; }

    IReadOnlySequence<T> CloneWithNewLength(int newLength);
  }

  public interface ISequence<T> : IReadOnlySequence<T> {
    new int Length { get; set; }
    new T this[int index] { get; set; }
    void Clear();
  }
}
