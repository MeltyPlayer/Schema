namespace schema.util.spans;

using System;

public static class SpanSplitExtensions {
  public static SpanSplitEnumerator<T> Split<T>(
      this ReadOnlySpan<T> span,
      T separator)
      where T : IEquatable<T>
    => new(span, separator);

  public ref struct SpanSplitEnumerator<T>(ReadOnlySpan<T> span, T separator)
      where T : IEquatable<T> {
    private ReadOnlySpan<T> remaining_ = span;
    private ReadOnlySpan<T> current_ = default;
    private bool isDone_ = false;

    public SpanSplitEnumerator<T> GetEnumerator() => this;

    public ReadOnlySpan<T> Current => this.current_;

    public bool MoveNext() {
      if (this.isDone_)
        return false;

      int idx = this.remaining_.IndexOf(separator);
      if (idx < 0) {
        this.current_ = this.remaining_;
        this.remaining_ = default;
        this.isDone_ = true;
        return true;
      }

      this.current_ = this.remaining_.Slice(0, idx);
      this.remaining_ = this.remaining_.Slice(idx + 1);
      return true;
    }
  }
}