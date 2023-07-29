using System.Collections;
using System.Collections.Generic;


namespace schema.binary.io {
  public interface INestedList<T> : IEnumerable<T> {
    void Add(T value);
    INestedList<T> Enter();
  }

  public class NestedList<T> : INestedList<T> {
    private class Node {
      public bool IsChild { get; set; }
      public NestedList<T> Child { get; set; }
      public List<T> List { get; set; }
    }

    private readonly LinkedList<Node> nodes_ = new();

    public void Add(T value) {
      List<T> list;
      if (this.nodes_.Count == 0 || this.nodes_.Last.Value.IsChild) {
        list = new List<T>();
        this.nodes_.AddLast(new Node {IsChild = false, List = list,});
      } else {
        list = this.nodes_.Last.Value.List;
      }

      list.Add(value);
    }

    public INestedList<T> Enter() {
      var child = new NestedList<T>();
      this.nodes_.AddLast(new Node {IsChild = true, Child = child});
      return child;
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public IEnumerator<T> GetEnumerator() {
      foreach (var node in this.nodes_) {
        if (node.IsChild) {
          foreach (var childElement in node.Child) {
            yield return childElement;
          }
        } else {
          foreach (var childElement in node.List) {
            yield return childElement;
          }
        }
      }
    }
  }
}