using System.Runtime.CompilerServices;

using schema.util.asserts;

namespace schema.text.reader;

public sealed partial class SchemaTextReader {
  private bool hasPeekedChar_;
  private char peekedChar_;

  private char PeekChar_() {
    if (this.hasPeekedChar_) {
      return this.peekedChar_;
    }

    var originalPosition = this.PositionInternal_;
    this.peekedChar_ = this.ReadChar();
    this.hasPeekedChar_ = true;
    this.PositionInternal_ = originalPosition;

    return this.peekedChar_;
  }

  private bool PeekCharAndProgressIfEqualTo_(char c) {
    if (this.hasPeekedChar_ && this.peekedChar_ == c) {
      this.ReadChar();
      return true;
    }

    if (!this.hasPeekedChar_) {
      var originalPosition = this.PositionInternal_;
      this.peekedChar_ = this.ReadChar();

      if (this.peekedChar_ == c) {
        return true;
      }

      this.hasPeekedChar_ = true;
      this.PositionInternal_ = originalPosition;
    }

    return false;
  }

  private bool PeekCharAndProgressIfNotEqualTo_(char c, out char peeked) {
    if (this.hasPeekedChar_ && this.peekedChar_ != c) {
      peeked = this.peekedChar_;
      this.ReadChar();
      return true;
    }

    if (!this.hasPeekedChar_) {
      var originalPosition = this.PositionInternal_;
      this.peekedChar_ = this.ReadChar();

      if (this.peekedChar_ != c) {
        peeked = this.peekedChar_;
        return true;
      }

      this.hasPeekedChar_ = true;
      this.PositionInternal_ = originalPosition;
    }

    peeked = default;
    return false;
  }

  public char ReadChar() {
    this.hasPeekedChar_ = false;
    var c = this.impl_.ReadChar();
    this.IncrementLineIndicesForChar_(c);
    return c;
  }

  public bool TryReadChar(out char c) {
    this.hasPeekedChar_ = false;
    if (this.impl_.TryReadChar(out c)) {
      this.IncrementLineIndicesForChar_(c);
      return true;
    }

    return false;
  }
}