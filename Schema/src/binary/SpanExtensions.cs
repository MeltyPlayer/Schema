using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace schema.binary {
  public static class SpanExtensions {
    private class ListDummy<T> {
      internal T[] Items;
    }

    public static Span<T> AsSpan<T>(this List<T> list) where T : unmanaged
      => Unsafe.As<ListDummy<T>>(list).Items.AsSpan(0, list.Count);
  }
}