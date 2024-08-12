using System.Collections.Generic;

using schema.binary;


namespace schema.util.sequences {
  public interface IReadOnlySequence<out TSelf, out T>
      : IEnumerable<T>, IBinaryConvertible
      where TSelf : IReadOnlySequence<TSelf, T> {
    T GetDefault();

    int Count { get; }
    T this[int index] { get; }

    TSelf CloneWithNewLength(int newLength);
  }

  public interface IConstLengthSequence<out TSelf, T>
      : IReadOnlySequence<TSelf, T> where TSelf : IReadOnlySequence<TSelf, T> {
    new T this[int index] { get; set; }
  }

  public interface ISequence<out TSelf, T> : IConstLengthSequence<TSelf, T>
      where TSelf : ISequence<TSelf, T> {
    void Clear();
    void ResizeInPlace(int newLength);
  }
}