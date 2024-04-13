using System.Collections;
using System.Collections.Generic;


namespace schema.util.data {
  public static class QueueExtensions {
    public static bool TryDequeue<T>(this Queue<T> queue, out T value) {
      if (queue.Count == 0) {
        value = default;
        return false;
      }

      value = queue.Dequeue();
      return true;
    }

    public static bool TryDequeue<T1, T2>(this Queue<(T1, T2)> queue,
                                          out T1 value1,
                                          out T2 value2) {
      if (queue.Count == 0) {
        value1 = default;
        value2 = default;
        return false;
      }

      (value1, value2) = queue.Dequeue();
      return true;
    }
  }
}