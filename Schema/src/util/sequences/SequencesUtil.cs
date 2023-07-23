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

    public static T[] CloneAndResizeSequence<T>(T[]? list, int length)
        where T : new() {
      SequencesUtil.AssertLengthNonnegative_(length);

      if (list != null && list.Length == length) {
        return list;
      }

      return list.Resized(length).ToArray();
    }

    public static ImmutableArray<T> CloneAndResizeSequence<T>(
        ImmutableArray<T>? list,
        int length) where T : new() {
      SequencesUtil.AssertLengthNonnegative_(length);

      if (list != null && list.Value.Length == length) {
        return list.Value;
      }

      return list.Resized<T>(length).ToImmutableArray();
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

    public static IList<T> CloneAndResizeSequence<T>(IList<T>? list, int length)
        where T : new() {
      SequencesUtil.AssertLengthNonnegative_(length);

      if (list != null && list.Count == length) {
        return list;
      }

      return list.Resized(length).ToList();
    }

    public static IReadOnlyList<T> CloneAndResizeSequence<T>(
        IReadOnlyList<T>? list,
        int length) where T : new() {
      SequencesUtil.AssertLengthNonnegative_(length);

      if (list != null && list.Count == length) {
        return list;
      }

      return (list?.Resized(length) ?? Enumerable.Repeat(new T(), length))
          .ToImmutableList();
    }


    public static void ResizeSequenceInPlace<TSequence, T>(
        ISequence<TSequence, T> list,
        int length) where TSequence : ISequence<TSequence, T> {
      SequencesUtil.AssertLengthNonnegative_(length);
      list.ResizeInPlace(length);
    }

    public static void CloneAndResizeSequence<TSequence, T>(
        IConstLengthSequence<TSequence, T> list,
        int length) where TSequence : IConstLengthSequence<TSequence, T> {
      SequencesUtil.AssertLengthNonnegative_(length);
      list.CloneWithNewLength(length);
    }

    public static void CloneAndResizeSequence<TSequence, T>(
        IReadOnlySequence<TSequence, T> list,
        int length) where TSequence : IReadOnlySequence<TSequence, T> {
      SequencesUtil.AssertLengthNonnegative_(length);
      list.CloneWithNewLength(length);
    }
  }
}