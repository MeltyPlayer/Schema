using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace schema.util.strings;

internal ref struct ValueStringBuilder {
  private char[]? arrayToReturnToPool_;
  private Span<char> chars_;
  private int pos_;

  public ValueStringBuilder(Span<char> initialBuffer) {
    this.arrayToReturnToPool_ = null;
    this.chars_ = initialBuffer;
    this.pos_ = 0;
  }

  public ValueStringBuilder(int initialCapacity) {
    this.arrayToReturnToPool_ = ArrayPool<char>.Shared.Rent(initialCapacity);
    this.chars_ = this.arrayToReturnToPool_;
    this.pos_ = 0;
  }

  public void Dispose() {
    char[]? toReturn = this.arrayToReturnToPool_;
    this = default;
    if (toReturn != null) {
      ArrayPool<char>.Shared.Return(toReturn);
    }
  }

  public void Clear() => this.pos_ = 0;


  public ReadOnlySpan<char> AsSpan() => this.chars_[..this.pos_];

  public override string ToString()
    => this.pos_ == 0
        ? string.Empty
        : this.AsSpan().ToString();

  public int Length {
    get => this.pos_;
    set => this.pos_ = value;
  }

  public void Append(char c) {
    this.GrowIfNeeded_(this.pos_ + 1);
    this.chars_[this.pos_++] = c;
  }

  public void Append(ReadOnlySpan<char> chars) {
    this.GrowIfNeeded_(this.pos_ + chars.Length);
    chars.CopyTo(this.chars_[this.pos_..]);
    this.pos_ += chars.Length;
  }

  [MethodImpl(MethodImplOptions.NoInlining)]
  private void GrowIfNeeded_(int neededLength) {
    if (neededLength < (uint) this.chars_.Length) {
      return;
    }

    int newCapacity = (int) Math.Max(
        (uint) neededLength,
        (uint) this.chars_.Length * 2);

    char[] poolArray = ArrayPool<char>.Shared.Rent(newCapacity);
    this.chars_[..this.pos_].CopyTo(poolArray);

    char[]? toReturn = this.arrayToReturnToPool_;
    this.chars_ = this.arrayToReturnToPool_ = poolArray;
    if (toReturn != null) {
      ArrayPool<char>.Shared.Return(toReturn);
    }
  }
}