using System.Collections.Generic;

namespace schema.util.enumerables {
  public static class EnumerableExtensions {
    public static IEnumerable<T> Yield<T>(this T value) {
      yield return value;
    }

    public static IEnumerable<T> Resized<T>(
        this IEnumerable<T>? enumerable,
        int length) where T : new() {
      var count = 0;

      if (enumerable != null) {
        foreach (var value in enumerable) {
          if (count >= length) {
            yield break;
          }

          yield return value;
          count++;
        }
      }

      while (count < length) {
        yield return new T();
        count++;
      }
    }
  }
}