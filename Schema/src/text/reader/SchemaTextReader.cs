using System;
using System.IO;

using schema.util.streams;

namespace schema.text.reader {
  public sealed partial class SchemaTextReader : ITextReader, IDisposable {
    private readonly ISeekableReadableStream baseStream_;

    public SchemaTextReader(Stream baseStream, int tabWidth = 4)
        : this(new ReadableStream(baseStream), tabWidth) { }

    public SchemaTextReader(ISeekableReadableStream baseStream, int tabWidth = 4) {
      this.baseStream_ = baseStream;
      this.TabWidth = tabWidth;
    }

    ~SchemaTextReader() => this.ReleaseUnmanagedResources_();

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
      T? valueInternal = default;
      bool success = false;

      this.AdvanceIfTrue(tr => {
        try {
          valueInternal = tr.ReadNew<T>();
          success = true;
          return true;
        } catch {
          return false;
        }
      });

      value = valueInternal;
      return success;
    }

    public void ReadNews<T>(out T[] array, int length)
        where T : ITextDeserializable, new()
      => array = this.ReadNews<T>(length);

    public T[] ReadNews<T>(int length)
        where T : ITextDeserializable, new() {
      var array = new T[length];
      for (var i = 0; i < length; ++i) {
        array[i] = this.ReadNew<T>();
      }

      return array;
    }
  }
}