using System;
using System.IO;
using System.Text;

using schema.util.streams;


namespace schema.text.reader;

public sealed partial class SchemaTextReader : ITextReader {
  private readonly UnbufferedStreamReader impl_;

  public SchemaTextReader(string text, int tabWidth = 4)
      : this(StreamReaderUtil.FromString(text), tabWidth) { }

  public SchemaTextReader(Stream baseStream, int tabWidth = 4)
      : this(baseStream, null, tabWidth) { }

  public SchemaTextReader(Stream baseStream,
                          Encoding? fallbackEncoding,
                          int tabWidth = 4)
      : this(new UnbufferedStreamReader(baseStream, fallbackEncoding),
             tabWidth) { }

  private SchemaTextReader(UnbufferedStreamReader impl, int tabWidth = 4) {
    this.impl_ = impl;
    this.TabWidth = tabWidth;
  }

  ~SchemaTextReader() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() => this.impl_.Dispose();

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