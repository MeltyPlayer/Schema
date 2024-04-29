using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace schema.binary {
  public sealed partial class SchemaBinaryReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AssertStrings_(ReadOnlySpan<char> expectedValue,
                                       ReadOnlySpan<char> actualValue) {
      if (!expectedValue.SequenceEqual(actualValue)) {
        var sb = new StringBuilder();
        throw new SchemaAssertionException(
            $"Expected {actualValue.ToString()} to be {expectedValue.ToString()}");
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Assert_<T>(T expectedValue, T actualValue) {
      if (!expectedValue.Equals(actualValue)) {
        throw new SchemaAssertionException(
            $"Expected {actualValue} to be {expectedValue}");
      }
    }

    private static void AssertAlmost_(double expectedValue,
                                      double actualValue,
                                      double delta = .01) {
      if (Math.Abs(expectedValue - actualValue) > delta) {
        throw new SchemaAssertionException(
            $"Expected {actualValue} to be {expectedValue}");
      }
    }

    private class SchemaAssertionException : Exception {
      public SchemaAssertionException(string message) : base(message) { }

      public SchemaAssertionException(string message, Exception innerException)
          : base(message, innerException) { }
    }
  }
}