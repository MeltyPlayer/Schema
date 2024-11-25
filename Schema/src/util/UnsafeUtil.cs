using System;
using System.Runtime.CompilerServices;


namespace schema.util;

public static class UnsafeUtil {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Span<T> AsSpan<T>(ref T value)
    => new(Unsafe.AsPointer(ref value), 1);
}