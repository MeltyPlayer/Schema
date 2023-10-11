using System;
using System.IO;

using schema.util.streams;

namespace schema.text.reader {
  public sealed partial class TextReader : ITextReader, IDisposable {
    private readonly ISeekableReadableStream baseStream_;

    public TextReader(Stream baseStream, int tabWidth = 4)
        : this(new ReadableStream(baseStream), tabWidth) { }

    public TextReader(ISeekableReadableStream baseStream, int tabWidth = 4) {
      this.baseStream_ = baseStream;
      this.TabWidth = tabWidth;
    }

    ~TextReader() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() => this.baseStream_.Dispose();

    public T ReadNew<T>() where T : ITextDeserializable, new() {
      var value = new T();
      value.Read(this);
      return value;
    }

    public bool TryReadNew<T>(out T? value)
        where T : ITextDeserializable, new() {
      var originalLineNumber = this.LineNumber;
      var originalIndexInLine = this.IndexInLine;
      var originalPosition = this.Position;

      try {
        value = this.ReadNew<T>();
        return true;
      } catch {
        this.LineNumber = originalLineNumber;
        this.IndexInLine = originalIndexInLine;
        this.Position = originalPosition;
        value = default;
        return false;
      }
    }

    public void ReadNewArray<T>(out T[] array, int length)
        where T : ITextDeserializable, new()
      => array = this.ReadNewArray<T>(length);

    public T[] ReadNewArray<T>(int length)
        where T : ITextDeserializable, new() {
      var array = new T[length];
      for (var i = 0; i < length; ++i) {
        array[i] = this.ReadNew<T>();
      }

      return array;
    }
  }
}