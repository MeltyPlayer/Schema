using System;
using System.IO;
using System.Runtime.CompilerServices;


namespace schema.text.reader;

public partial class SchemaTextReader {
  public int TabWidth { get; } = 4;
  public int LineNumber { get; private set; }
  public int IndexInLine { get; private set; }

  public long Position {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.PositionInternal_;
    set {
      if (value == this.PositionInternal_) {
        return;
      }

      long bytesToRead;
      if (value > this.PositionInternal_) {
        bytesToRead = value - this.PositionInternal_;
      } else {
        this.IndexInLine = 0;
        this.LineNumber = 0;
        this.PositionInternal_ = 0;

        bytesToRead = value;
      }

      var charsToRead = this.impl_.Encoding.GetMaxCharCount((int) bytesToRead);
      for (var i = 0; i < charsToRead; i++) {
        if (this.PositionInternal_ >= value) {
          break;
        }

        this.ReadChar();
      }
    }
  }

  private long PositionInternal_ {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.impl_.Position;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set => this.impl_.Position = value;
  }

  public long Length {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.impl_.Length;
  }

  public bool Eof {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.impl_.Eof;
  }

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