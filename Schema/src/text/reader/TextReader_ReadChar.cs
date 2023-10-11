using schema.util.streams;

namespace schema.text.reader {
  public sealed partial class TextReader {
    private char? peekedChar_;

    private char PeekChar_() {
      if (this.peekedChar_ != null) {
        return this.peekedChar_.Value;
      }

      var originalLineNumber = this.LineNumber;
      var originalIndexInLine = this.IndexInLine;
      var originalPosition = this.PositionInternal_;

      this.peekedChar_ = this.ReadChar();

      this.LineNumber = originalLineNumber;
      this.IndexInLine = originalIndexInLine;
      this.PositionInternal_ = originalPosition;

      return this.peekedChar_.Value;
    }

    // TODO: Handle other encodings besides ASCII
    public char ReadChar() {
      this.peekedChar_ = null;
      var c = (char) this.baseStream_.ReadByte();
      this.IncrementLineIndicesForChar_(c);
      return c;
    }
  }
}