using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;


namespace schema.util.asserts;

public sealed class Asserts {
  /**
   * NOTE: Using $"" to define messages allocates strings, and can be expensive!
   * Try to avoid allocating strings unless an assertion actually fails.
   */
  public sealed class AssertionException : Exception {
    public AssertionException(string message) : base(message) { }

    public override string StackTrace {
      get {
        List<string> stackTrace = [];
        stackTrace.AddRange(base.StackTrace!.Split(
                                [Environment.NewLine],
                                StringSplitOptions.None));

        var assertLine = new Regex("\\s*Asserts\\.");
        stackTrace.RemoveAll(x => assertLine.IsMatch(x));

        return string.Join(Environment.NewLine, stackTrace.ToArray());
      }
    }
  }

  public static bool Fail(string? message = null)
    => throw new AssertionException(message ?? "Failed.");

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool True(bool value, string? message = null)
    => value || Fail(message ?? "Expected to be true.");

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool False(bool value, string? message = null)
    => True(!value, message ?? "Expected to be false.");

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool Nonnull(
      object? instance,
      string? message = null)
    => True(instance != null,
            message ?? "Expected reference to be nonnull.");

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T CastNonnull<T>(
      T? instance,
      string? message = null) {
    True(instance != null,
         message ?? "Expected reference to be nonnull.");
    return instance!;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Null(
      object? instance,
      string message = "Expected reference to be null.")
    => True(instance == null, message);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool Same(
      object instanceA,
      object instanceB,
      string message = "Expected references to be the same.")
    => True(ReferenceEquals(instanceA, instanceB), message);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Equal<TEnumerable>(
      TEnumerable enumerableA,
      TEnumerable enumerableB) where TEnumerable : IEnumerable {
    var enumeratorA = enumerableA.GetEnumerator();
    var enumeratorB = enumerableB.GetEnumerator();

    var hasA = enumeratorA.MoveNext();
    var hasB = enumeratorB.MoveNext();

    var index = 0;
    while (hasA && hasB) {
      var currentA = enumeratorA.Current;
      var currentB = enumeratorB.Current;

      if (!Equals(currentA, currentB)) {
        Fail($"Expected {currentA} to equal {currentB} at index ${index}.");
      }

      index++;

      hasA = enumeratorA.MoveNext();
      hasB = enumeratorB.MoveNext();
    }

    True(!hasA && !hasB,
         "Expected enumerables to be the same length.");
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool AllEqual<T>(params T[] values) where T : notnull {
    if (values.Length <= 1) {
      return true;
    }

    var first = values[0];
    foreach (var value in values.Skip(1)) {
      if (!Equal(first, value)) {
        return false;
      }
    }

    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool Equal<T>(
      T expected,
      T actual,
      string? message = null) where T : notnull {
    if (expected.Equals(actual)) {
      return true;
    }

    Fail(message ?? $"Expected {actual} to equal {expected}.");
    return false;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool Equal(
      string expected,
      string actual,
      string? message = null)
    => Equal<string>(expected, actual, message);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TExpected AsA<TExpected>(
      object? instance,
      string? message = null) {
    if (instance is TExpected expected) {
      return expected;
    }

    Asserts.Fail(message);
    return default!;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Assert<T>(T? value) where T : notnull {
    Nonnull(value);
    return value!;
  }
}