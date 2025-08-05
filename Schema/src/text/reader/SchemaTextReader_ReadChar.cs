using System.Runtime.InteropServices.ComTypes;

namespace schema.text.reader;

public sealed partial class SchemaTextReader {
  private char? peekedChar_;

  private char PeekChar_() {
    if (this.peekedChar_ != null) {
      return this.peekedChar_.Value;
    }

    var originalPosition = this.PositionInternal_;
    this.peekedChar_ = (char) this.baseStream_.ReadByte();
    this.PositionInternal_ = originalPosition;

    return this.peekedChar_.Value;
  }

  private bool PeekCharAndProgressIfEqualTo_(char c) {
    if (this.peekedChar_ == c) {
      this.peekedChar_ = null;
      this.ReadChar();
      return true;
    }

    if (this.peekedChar_ == null) {
      var originalPosition = this.PositionInternal_;
      this.peekedChar_ = (char) this.baseStream_.ReadByte();

      if (this.peekedChar_ == c) {
        this.peekedChar_ = null;
        return true;
      }

      this.PositionInternal_ = originalPosition;
      return false;
    }

    return false;
  }

  private bool PeekCharAndProgressIfNotEqualTo_(char c, out char peeked) {
    if (this.peekedChar_ != null && this.peekedChar_ != c) {
      peeked = this.peekedChar_.Value;
      this.peekedChar_ = null;
      this.ReadChar();
      return true;
    }

    if (this.peekedChar_ == null) {
      var originalPosition = this.PositionInternal_;
      this.peekedChar_ = (char) this.baseStream_.ReadByte();

      if (this.peekedChar_ != c) {
        peeked = this.peekedChar_.Value;
        this.peekedChar_ = null;
        return true;
      }

      peeked = default;
      this.PositionInternal_ = originalPosition;
      return false;
    }

    peeked = default;
    return false;
  }

  // TODO: Handle other encodings besides ASCII
  public char ReadChar() {
    this.peekedChar_ = null;
    var c = (char) this.baseStream_.ReadByte();
    this.IncrementLineIndicesForChar_(c);
    return c;
  }
}