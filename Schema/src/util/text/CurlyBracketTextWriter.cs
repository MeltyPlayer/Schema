using System;
using System.IO;

namespace schema.util.text {
  public interface ICurlyBracketTextWriter {
    public ICurlyBracketTextWriter EnterBlock(string prefix = "");
    public ICurlyBracketTextWriter Write(string text);
    public ICurlyBracketTextWriter WriteLine(string text);
    public ICurlyBracketTextWriter ExitBlock();
  }

  public sealed class CurlyBracketTextWriter
      : ICurlyBracketTextWriter,
        IDisposable {
    private readonly TextWriter impl_;
    private int indentLevel_ = 0;
    private bool hasIndentedOnCurrentLine_ = false;

    public CurlyBracketTextWriter(TextWriter impl) {
      this.impl_ = impl;
    }

    ~CurlyBracketTextWriter() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() {
      this.impl_.Dispose();
    }

    public ICurlyBracketTextWriter EnterBlock(string prefix = "") {
      if (prefix.Length > 0) {
        prefix = $"{prefix} ";
      }

      this.WriteLine($"{prefix}{{");
      return this;
    }

    public ICurlyBracketTextWriter Write(string text) {
      var lines = text.Split('\n');
      for (var i = 0; i < lines.Length; ++i) {
        var line = lines[i];
        var isLastLine = !(i < lines.Length - 1);

        if (isLastLine && line.Length == 0) {
          break;
        }

        foreach (var c in line) {
          if (c == '}') {
            --this.indentLevel_;

            if (this.indentLevel_ < 0) {
              throw new Exception("Exited an extra block!");
            }
          }
        }

        if (!isLastLine) {
          this.TryToPrintIndent_();
          this.impl_.WriteLine(line);
          this.hasIndentedOnCurrentLine_ = false;
        } else {
          this.TryToPrintIndent_();
          this.impl_.Write(line);
        }

        foreach (var c in line) {
          if (c == '{') {
            ++this.indentLevel_;
          }
        }
      }

      return this;
    }

    public ICurlyBracketTextWriter ExitBlock() {
      this.WriteLine("}");
      return this;
    }

    public ICurlyBracketTextWriter WriteLine(string text) => Write(text + '\n');

    private void TryToPrintIndent_() {
      if (this.hasIndentedOnCurrentLine_) {
        return;
      }

      this.hasIndentedOnCurrentLine_ = true;
      for (var i = 0; i < this.indentLevel_; ++i) {
        this.impl_.Write("  ");
      }
    }
  }
}