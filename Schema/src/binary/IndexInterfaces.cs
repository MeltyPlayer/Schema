using System.Collections;
using System.Collections.Generic;


namespace schema.binary {
  public interface IIndexedElement {
    int Index { get; internal set; }
  }

  public interface IReadonlySchemaList<TIndexedElement> :
      IReadOnlyList<TIndexedElement>
      where TIndexedElement : IIndexedElement { }

  public interface ISchemaList<TIndexedElement> :
      IReadonlySchemaList<TIndexedElement>,
      IList<TIndexedElement>
      where TIndexedElement : IIndexedElement { }


  public class SchemaList<TIndexedElement> : ISchemaList<TIndexedElement>
      where TIndexedElement : IIndexedElement {
    private readonly List<TIndexedElement> impl_ = new();

    int ICollection<TIndexedElement>.Count => this.Length;
    int IReadOnlyCollection<TIndexedElement>.Count => this.Length;
    public int Length => this.impl_.Count;

    public int IndexOf(TIndexedElement item) => this.impl_.IndexOf(item);

    public TIndexedElement this[int index] {
      get => this.impl_[index];
      set {
        this.impl_[index] = value;
        value.Index = index;
      }
    }

    public void Insert(int index, TIndexedElement item) {
      this.impl_.Insert(index, item);
      this.UpdateIndexes_();
    }

    public void Add(TIndexedElement item) {
      item.Index = this.impl_.Count;
      this.impl_.Add(item);
    }

    public void RemoveAt(int index) {
      this.impl_.RemoveAt(index);
      this.UpdateIndexes_();
    }

    public bool Remove(TIndexedElement item) {
      var returnValue = this.impl_.Remove(item);
      this.UpdateIndexes_();
      return returnValue;
    }

    private void UpdateIndexes_() {
      for (var i = 0; i < this.Length; ++i) {
        var value = this.impl_[i];
        value.Index = i;
        this.impl_[i] = value;
      }
    }

    public void Clear() => this.impl_.Clear();

    public bool Contains(TIndexedElement item) => this.impl_.Contains(item);

    public void CopyTo(TIndexedElement[] array, int arrayIndex)
      => this.impl_.CopyTo(array, arrayIndex);

    public bool IsReadOnly => false;

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public IEnumerator<TIndexedElement> GetEnumerator()
      => this.impl_.GetEnumerator();
  }
}