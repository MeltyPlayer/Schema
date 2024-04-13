using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using schema.util.asserts;

namespace schema.util.generators {
  public interface ISourceFileDictionary {
    void Add(string fileName, string source);
    void SetHandler(Action<string, string> handler);
  }

  public class SourceFileDictionary : ISourceFileDictionary {
    private readonly HashSet<string> fileNames_ = new();
    private readonly ConcurrentDictionary<string, string> sourceByFileName_
        = new();

    private Action<string, string>? handler_;

    public void Add(string fileName, string source) {
      Asserts.True(this.fileNames_.Add(fileName));

      if (this.handler_ != null) {
        this.handler_(fileName, source);
        return;
      }

      this.sourceByFileName_[fileName] = source;
    }

    public void SetHandler(Action<string, string> handler) {
      this.handler_ = handler;

      foreach (var fileNameAndSource in this.sourceByFileName_) {
        handler(fileNameAndSource.Key, fileNameAndSource.Value);
      }

      this.sourceByFileName_.Clear();
    }
  }
}