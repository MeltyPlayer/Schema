using System.Runtime.CompilerServices;


namespace schema.binary {
  public sealed partial class SchemaBinaryReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T ReadNew<T>() where T : IBinaryDeserializable, new() {
      this.AssertNotEof();
      var value = new T();
      value.Read(this);
      return value;
    }

    public bool TryReadNew<T>(out T? value)
        where T : IBinaryDeserializable, new() {
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadNewArray<T>(out T[] array, int length)
        where T : IBinaryDeserializable, new() {
      array = ReadNewArray<T>(length);
    }

    public T[] ReadNewArray<T>(int length)
        where T : IBinaryDeserializable, new() {
      var array = new T[length];
      for (var i = 0; i < length; ++i) {
        this.AssertNotEof();
        array[i] = this.ReadNew<T>();
      }
      return array;
    }
  }
}