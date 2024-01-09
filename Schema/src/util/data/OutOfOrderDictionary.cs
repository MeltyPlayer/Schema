using System.Collections.Generic;
using System.Threading.Tasks;

namespace schema.util.data {
  public interface IOutOfOrderDictionary<in TKey, TValue>
      where TKey : notnull {
    Task<TValue> Get(TKey key);
    void Set(TKey key, TValue value);
    void Set(TKey key, Task<TValue> value);
  }

  public class OutOfOrderDictionary<TKey, TValue>
      : IOutOfOrderDictionary<TKey, TValue> {
    private readonly Dictionary<TKey, TaskCompletionSource<TValue>> impl_ =
        new();

    public Task<TValue> Get(TKey key)
      => this.GetOrCreateTaskCompletionSource_(key).Task;

    public void Set(TKey key, TValue value) {
      this.GetOrCreateTaskCompletionSource_(key).SetResult(value);
    }

    public void Set(TKey key, Task<TValue> valueTask) {
      var taskCompletionSource = this.GetOrCreateTaskCompletionSource_(key);
      Task.Run(async () => {
        var value = await valueTask;
        taskCompletionSource.SetResult(value);
      });
    }

    private TaskCompletionSource<TValue> GetOrCreateTaskCompletionSource_(
        TKey key) {
      if (!this.impl_.TryGetValue(key, out var taskCompletionSource)) {
        taskCompletionSource = new TaskCompletionSource<TValue>();
        this.impl_[key] = taskCompletionSource;
      }

      return taskCompletionSource;
    }
  }
}