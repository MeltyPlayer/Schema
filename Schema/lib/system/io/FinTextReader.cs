using schema.text;

namespace System.IO {
  public sealed partial class FinTextReader : ITextReader {
    private readonly Stream baseStream_;

    public FinTextReader(Stream baseStream) {
      this.baseStream_ = baseStream;
    }

    ~FinTextReader() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() => this.baseStream_.Dispose();

    public long Position {
      get => this.baseStream_.Position;
      set => this.baseStream_.Position = value;
    }

    public long Length => this.baseStream_.Length;
    public bool Eof => this.Position >= this.Length;


    public T ReadNew<T>() where T : ITextDeserializable, new() {
      var value = new T();
      value.Read(this);
      return value;
    }

    public bool TryReadNew<T>(out T? value) where T : ITextDeserializable, new() {
      var originalPosition = this.Position;
      try {
        value = this.ReadNew<T>();
        return true;
      } catch {
        this.Position = originalPosition;
        value = default;
        return false;
      }
    }

    public void ReadNewArray<T>(out T[] array, int length)
        where T : ITextDeserializable, new()
      => array = ReadNewArray<T>(length);

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