using System;
using System.Runtime.CompilerServices;

namespace schema.text.reader {
  public partial class TextReader {
    public int TabWidth { get; } = 4;
    public int LineNumber { get; private set; }
    public int IndexInLine { get; private set; }

    public long Position {
      get => this.PositionInternal_;
      set {
        if (value == this.PositionInternal_) {
          return;
        }

        long charsToRead;
        if (value > this.PositionInternal_) {
          charsToRead = value - this.PositionInternal_;
        } else {
          this.IndexInLine = 0;
          this.LineNumber = 0;
          this.PositionInternal_ = 0;

          charsToRead = value;
        }

        for (var i = 0; i < charsToRead; i++) {
          this.ReadChar();
        }
      }
    }

    private long PositionInternal_ {
      get => this.baseStream_.Position;
      set => this.baseStream_.Position = value;
    }

    public long Length => this.baseStream_.Length;
    public bool Eof => this.PositionInternal_ >= this.Length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void IncrementLineIndicesForChar_(char c) {
      if (c == '\n') {
        this.IndexInLine = 0;
        ++this.LineNumber;
      } else if (c == '\t') {
        var remainderToNextTabPosition = this.IndexInLine % this.TabWidth;
        this.IndexInLine += this.TabWidth - remainderToNextTabPosition;
      } else if (!char.IsControl(c)) {
        ++this.IndexInLine;
      }
    }

    public void AdvanceIfTrue(Func<ITextReader, bool> handler) {
      var originalLineNumber = this.LineNumber;
      var originalIndexInLine = this.IndexInLine;
      var originalPosition = this.PositionInternal_;

      if (handler(this)) {
        return;
      }

      this.LineNumber = originalLineNumber;
      this.IndexInLine = originalIndexInLine;
      this.PositionInternal_ = originalPosition;
    }
  }
}