﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace schema.binary.util {
  public class Asserts {
    /**
     * NOTE: Using $"" to define messages allocates strings, and can be expensive!
     * Try to avoid allocating strings unless an assertion actually fails.
     */

    public class AssertionException : Exception {
      public AssertionException(string message) : base(message) { }

      public override string StackTrace {
        get {
          List<string> stackTrace = new List<string>();
          stackTrace.AddRange(base.StackTrace!.Split(
                                  new string[] { Environment.NewLine },
                                  StringSplitOptions.None));

          var assertLine = new Regex("\\s*Asserts\\.");
          stackTrace.RemoveAll(x => assertLine.IsMatch(x));

          return string.Join(Environment.NewLine, stackTrace.ToArray());
        }
      }
    }

    public static bool Fail(string? message = null)
      => throw new AssertionException(message ?? "Failed.");

    public static bool True(bool value, string? message = null)
      => value || Asserts.Fail(message ?? "Expected to be true.");

    public static bool False(bool value, string? message = null)
      => Asserts.True(!value, message ?? "Expected to be false.");

    public static bool Nonnull(
        object? instance,
        string? message = null)
      => Asserts.True(instance != null,
                      message ?? "Expected reference to be nonnull.");

    public static T CastNonnull<T>(
        T? instance,
        string? message = null) {
      Asserts.True(instance != null,
                   message ?? "Expected reference to be nonnull.");
      return instance!;
    }

    public static void Null(
        object? instance,
        string message = "Expected reference to be null.")
      => Asserts.True(instance == null, message);

    public static bool Same(
        object instanceA,
        object instanceB,
        string message = "Expected references to be the same.")
      => Asserts.True(object.ReferenceEquals(instanceA, instanceB), message);

    public static void Different(
        object instanceA,
        object instanceB,
        string message = "Expected references to be different.") {
      Asserts.False(object.ReferenceEquals(instanceA, instanceB), message);
    }

    public static bool Equal(
        object? expected,
        object? actual,
        string? message = null) {
      if (expected?.Equals(actual) ?? false) {
        return true;
      }
      Asserts.Fail(message ?? $"Expected {actual} to equal {expected}.");
      return false;
    }

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

        if (!object.Equals(currentA, currentB)) {
          Asserts.Fail($"Expected {currentA} to equal {currentB} at index ${index}.");
        }
        index++;

        hasA = enumeratorA.MoveNext();
        hasB = enumeratorB.MoveNext();
      }

      Asserts.True(!hasA && !hasB,
                   "Expected enumerables to be the same length.");
    }

    public static bool Equal<T>(
        T expected,
        T actual,
        string? message = null) {
      if (expected?.Equals(actual) ?? false) {
        return true;
      }
      Asserts.Fail(message ?? $"Expected {actual} to equal {expected}.");
      return false;
    }

    public static bool Equal(
        string expected,
        string actual,
        string? message = null)
      => Asserts.Equal<string>(expected, actual, message);

    public static bool IsA<TExpected>(object? instance, string? message = null)
      => Asserts.IsA(instance, typeof(TExpected), message);

    public static bool IsA(
        object? instance,
        Type expected,
        string? message = null)
      => Asserts.Nonnull(instance, message) &&
         Asserts.Equal(instance!.GetType(), expected, message);

    public static TExpected AsA<TExpected>(
        object? instance,
        string? message = null) {
      Asserts.IsA<TExpected>(instance, message);
      return (TExpected)instance!;
    }

    public static T Assert<T>(T? value) where T : notnull {
      Asserts.Nonnull(value);
      return value!;
    }
  }
}