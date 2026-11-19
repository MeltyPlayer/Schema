using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace schema.util.strings;

internal ref struct ValueStringBuilder {
  private char[]? arrayToReturnToPool_;
  private Span<char> chars_;
  private int pos_;

  public ValueStringBuilder(Encoding encoding, Span<char> initialBuffer) {
    this.arrayToReturnToPool_ = null;
    this.chars_ = initialBuffer;
    this.pos_ = 0;
  }

  public ValueStringBuilder(Encoding encoding, int initialCapacity) {
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
    int pos = this.pos_;
    if ((uint) pos < (uint) this.chars_.Length) {
      this.chars_[pos] = c;
      this.pos_ = pos + 1;
    } else {
      this.GrowAndAppend_(c);
    }
  }

  [MethodImpl(MethodImplOptions.NoInlining)]
  private void GrowAndAppend_(char b) {
    this.Grow_(1);
    this.Append(b);
  }

  [MethodImpl(MethodImplOptions.NoInlining)]
  private void Grow_(int additionalCapacityBeyondPos) {
    int newCapacity = (int) Math.Max(
        (uint) (this.pos_ + additionalCapacityBeyondPos),
        (uint) this.chars_.Length * 2);

    char[] poolArray = ArrayPool<char>.Shared.Rent(newCapacity);
    this.chars_.Slice(0, this.pos_).CopyTo(poolArray);

    char[]? toReturn = this.arrayToReturnToPool_;
    this.chars_ = this.arrayToReturnToPool_ = poolArray;
    if (toReturn != null) {
      ArrayPool<char>.Shared.Return(toReturn);
    }
  }
}