using System.Collections.Generic;
using System.Threading.Tasks;

using schema.binary.util;


namespace schema.binary.io {
  public interface IOutOfOrderDictionary<in TKey, TValue>
      where TKey : notnull {
    void AssertAllPopulated();
    void AssertAllCompleted();

    void Clear();

    Task<TValue> Get(TKey key);
    void Set(TKey key, TValue value);
  }

  public class OutOfOrderDictionary<TKey, TValue>
      : IOutOfOrderDictionary<TKey, TValue> {
    private readonly Dictionary<TKey, bool> populated_ = new();
    private readonly Dictionary<TKey, TaskCompletionSource<TValue>> impl_ =
        new();

    public void AssertAllPopulated() {
      var incompleted = new List<TKey>();
      foreach (var pair in this.populated_) {
        if (!pair.Value) {
          incompleted.Add(pair.Key);
        }
      }

      if (incompleted.Count > 0) {
        Asserts.Fail(
            $"Expected for all keys in the out-of-order dictionary to be populated values, but still has {incompleted.Count}/{this.impl_.Count} waiting!");
      }
    }

    public void AssertAllCompleted() {
      var incompleted = new List<TKey>();
      foreach (var pair in this.impl_) {
        if (!pair.Value.Task.IsCompleted) {
          incompleted.Add(pair.Key);
        }
      }

      if (incompleted.Count > 0) {
        Asserts.Fail(
            $"Expected for all keys in the out-of-order dictionary to be completed, but still has {incompleted.Count}/{this.impl_.Count
            } waiting!");
      }
    }

    public void Clear() => this.impl_.Clear();


    public Task<TValue> Get(TKey key)
      => this.GetOrCreateTaskCompletionSource_(key).Task;

    public void Set(TKey key, TValue value) {
      this.GetOrCreateTaskCompletionSource_(key).SetResult(value);
      this.populated_[key] = true;
    }

    public void Set(TKey key, Task<TValue> valueTask) {
      var taskCompletionSource = this.GetOrCreateTaskCompletionSource_(key);
      Task.Run(async () => {
        var value = await valueTask;
        taskCompletionSource.SetResult(value);
      });
      this.populated_[key] = true;
    }

    private TaskCompletionSource<TValue> GetOrCreateTaskCompletionSource_(
        TKey key) {
      if (!this.impl_.TryGetValue(key, out var taskCompletionSource)) {
        taskCompletionSource = new TaskCompletionSource<TValue>();
        this.populated_[key] = false;
        this.impl_[key] = taskCompletionSource;
      }

      return taskCompletionSource;
    }
  }
}