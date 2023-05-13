using System.Collections;
using System.Collections.Generic;


namespace schema.binary.io {
  public enum UpDownDirection {
    UPWARD,
    DOWNWARD
  }

  public interface IUpDownStackNode<T> {
    UpDownDirection Direction { get; }
  }

  public interface IUpStackNode<T> : IUpDownStackNode<T> {
    T FromValue { get; }
  }

  public interface IDownStackNode<T> : IUpDownStackNode<T> {
    T ToValue { get; }
  }

  public interface IUpDownStack<T> : IEnumerable<IUpDownStackNode<T>> {
    int Count { get; }

    bool TryPeek(out T value);
    void PushUpFrom(T value);
    void PushDownTo(T value);
  }

  public class UpDownStack<T> : IUpDownStack<T> where T : notnull {
    private readonly Stack<IUpDownStackNode<T>> impl_ = new();

    public int Count => this.impl_.Count;

    public bool TryPeek(out T value) {
      if (this.Count == 0) {
        value = default;
        return false;
      }

      value = this.impl_.Peek() switch {
          IDownStackNode<T> downStackNode => downStackNode.ToValue,
          IUpStackNode<T> upStackNode     => upStackNode.FromValue,
      };
      return true;
    }

    public void PushUpFrom(T value)
      => this.PushOrPopInDirection_(UpDownDirection.UPWARD, value);

    public void PushDownTo(T value)
      => this.PushOrPopInDirection_(UpDownDirection.DOWNWARD, value);

    private void PushOrPopInDirection_(
        UpDownDirection direction,
        T value) {
      if (this.CanReturnBackToPreviousValue_(direction, value)) {
        this.impl_.Pop();
        return;
      }

      this.impl_.Push(direction switch {
          UpDownDirection.UPWARD   => new UpStackNode {FromValue = value},
          UpDownDirection.DOWNWARD => new DownStackNode {ToValue = value},
      });
    }

    private bool CanReturnBackToPreviousValue_(
        UpDownDirection direction,
        T value) {
      if (this.Count == 0) {
        return false;
      }

      var last = this.impl_.Peek();
      if (last.Direction == direction) {
        return false;
      }

      return value.Equals(last switch {
          IDownStackNode<T> downStackNode => downStackNode.ToValue,
          IUpStackNode<T> upStackNode     => upStackNode.FromValue,
      });
    }


    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public IEnumerator<IUpDownStackNode<T>> GetEnumerator()
      => this.impl_.GetEnumerator();


    private class UpStackNode : IUpStackNode<T> {
      public UpDownDirection Direction => UpDownDirection.UPWARD;
      public T FromValue { get; set; }
    }

    private class DownStackNode : IDownStackNode<T> {
      public UpDownDirection Direction => UpDownDirection.DOWNWARD;
      public T ToValue { get; set; }
    }
  }
}