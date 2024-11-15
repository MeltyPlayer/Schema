using System;
using System.IO;
using System.Runtime.CompilerServices;

using schema.util.asserts;


namespace schema.binary;

public sealed partial class SchemaBinaryReader {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public T ReadNew<T>() where T : IBinaryDeserializable, new() {
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
    } catch (SchemaAssertionException) {
      this.Position = originalPosition;
      value = default;
      return false;
    } catch (Asserts.AssertionException) {
      this.Position = originalPosition;
      value = default;
      return false;
    } catch (EndOfStreamException) {
      this.Position = originalPosition;
      value = default;
      return false;
    } catch {
      this.Position = originalPosition;
      throw;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public T[] ReadNews<T>(int length)
      where T : IBinaryDeserializable, new() {
    var array = new T[length];
    this.ReadNews<T>(array);
    return array;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ReadNews<T>(Span<T> dst)
      where T : IBinaryDeserializable, new() {
    for (var i = 0; i < dst.Length; ++i) {
      dst[i] = this.ReadNew<T>();
    }
  }
}