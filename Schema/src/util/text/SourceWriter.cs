using System;
using System.IO;

using CommunityToolkit.HighPerformance;

namespace schema.util.text {
  public interface ISourceWriter : IDisposable {
    ISourceWriter Write(string text);
  }

  public sealed class SourceWriter(TextWriter impl) : ISourceWriter {
    private int indentLevel_;
    private bool hasIndentedOnCurrentLine_;

    ~SourceWriter() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() {
      impl.Dispose();
    }

    public ISourceWriter Write(string text) {
      var lines = text.Split('\n');
      for (var i = 0; i < lines.Length; ++i) {
        var line = lines[i];
        var isLastLine = !(i < lines.Length - 1);

        if (isLastLine && line.Length == 0) {
          break;
        }

        this.indentLevel_ -= line.Count('}');
        if (this.indentLevel_ < 0) {
          throw new Exception("Exited an extra block!");
        }

        if (!isLastLine) {
          this.TryToPrintIndent_();
          impl.WriteLine(line);
          this.hasIndentedOnCurrentLine_ = false;
        } else {
          this.TryToPrintIndent_();
          impl.Write(line);
        }

        this.indentLevel_ += line.Count('{');
      }

      return this;
    }

    private void TryToPrintIndent_() {
      if (this.hasIndentedOnCurrentLine_) {
        return;
      }

      this.hasIndentedOnCurrentLine_ = true;
      for (var i = 0; i < this.indentLevel_; ++i) {
        impl.Write("  ");
      }
    }
  }
}