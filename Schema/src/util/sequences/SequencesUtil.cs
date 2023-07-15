using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using schema.util.enumerables;

namespace schema.util.sequences {
  public static class SequencesUtil {
    private static void AssertLengthNonnegative_(int length) {
      if (length < 0) {
        throw new Exception("Expected length to be nonnegative!");
      }
    }

    public static T[] ResizeSequence<T>(T[]? list, int length) where T : new() {
      SequencesUtil.AssertLengthNonnegative_(length);

      if (list != null && list.Length == length) {
        return list;
      }

      return (list?.Resized(length) ?? Enumerable.Repeat(new T(), length))
          .ToArray();
    }

    public static ImmutableArray<T> ResizeSequence<T>(
        ImmutableArray<T>? list,
        int length) where T : new() {
      SequencesUtil.AssertLengthNonnegative_(length);

      if (list != null && list.Value.Length == length) {
        return list.Value;
      }

      return (list?.Resized(length) ?? Enumerable.Repeat(new T(), length))
          .ToImmutableArray();
    }


    public static void ResizeSequenceInPlace<T>(IList<T> list, int length)
        where T : new() {
      SequencesUtil.AssertLengthNonnegative_(length);
      while (list.Count > length) {
        list.RemoveAt(list.Count - 1);
      }

      while (list.Count < length) {
        list.Add(new T());
      }
    }

    public static IReadOnlyList<T> ResizeSequence<T>(IReadOnlyList<T>? list,
      int length) where T : new() {
      SequencesUtil.AssertLengthNonnegative_(length);

      if (list != null && list.Count == length) {
        return list;
      }

      return (list?.Resized(length) ?? Enumerable.Repeat(new T(), length))
          .ToArray();
    }


    public static void ResizeSequenceInPlace<T>(ISequence<T> list, int length) {
      SequencesUtil.AssertLengthNonnegative_(length);
      list.Count = length;
    }

    public static IReadOnlySequence<T> ResizeSequence<T>(
        IReadOnlySequence<T> list,
        int length) {
      SequencesUtil.AssertLengthNonnegative_(length);
      return list.CloneWithNewLength(length);
    }
  }
}