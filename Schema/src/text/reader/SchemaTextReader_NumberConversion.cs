using System;
using System.Runtime.CompilerServices;

using csFastFloat;

using schema.util.asserts;


namespace schema.text.reader;

public sealed partial class SchemaTextReader {
  private byte ConvertByte_(ReadOnlySpan<char> span)
    => (byte) ParseUlongLessThan_(span, byte.MaxValue);

  private sbyte ConvertSByte_(ReadOnlySpan<char> span)
    => (sbyte) ParseLongWithinRange(span, sbyte.MinValue, sbyte.MaxValue);

  private short ConvertInt16_(ReadOnlySpan<char> span)
    => (short) ParseLongWithinRange(span, short.MinValue, short.MaxValue);

  private ushort ConvertUInt16_(ReadOnlySpan<char> span)
    => (ushort) ParseUlongLessThan_(span, ushort.MaxValue);

  private int ConvertInt32_(ReadOnlySpan<char> span)
    => (int) ParseLongWithinRange(span, int.MinValue, int.MaxValue);

  private uint ConvertUInt32_(ReadOnlySpan<char> span)
    => (uint) ParseUlongLessThan_(span, uint.MaxValue);

  private long ConvertInt64_(ReadOnlySpan<char> span)
    => ParseLongWithinRange(span, long.MinValue, long.MaxValue);

  private ulong ConvertUInt64_(ReadOnlySpan<char> span) => ParseUlong_(span);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float ConvertSingle_(ReadOnlySpan<char> span)
    => FastFloatParser.ParseFloat(span);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private double ConvertDouble_(ReadOnlySpan<char> span)
    => FastDoubleParser.ParseDouble(span);

  public static long ParseLongWithinRange(
      ReadOnlySpan<char> span,
      long minValue,
      long maxValue) {
    Asserts.False(span.IsEmpty, "Span was empty");

    int i = 0;
    bool negative = false;
    if (span[0] == '-') {
      negative = true;
      i = 1;

      if (span.Length == 1) {
        Asserts.Fail("Span was only '-'");
      }
    } else if (span[0] == '+') {
      i = 1;
      if (span.Length == 1) {
        Asserts.Fail("Span was only '+'");
      }
    }

    return negative
        ? -1 *
          (long) ParseUlongLessThan_(span[i..], unchecked((ulong) -minValue))
        : (long) ParseUlongLessThan_(span[i..], (ulong) maxValue);
  }

  public static ulong ParseUlongLessThan_(
      ReadOnlySpan<char> span,
      ulong maxValue) {
    var value = ParseUlong_(span);
    if (value > maxValue) {
      Asserts.Fail($"Expected to read value less than: {maxValue}");
    }

    return value;
  }

  public static ulong ParseUlong_(ReadOnlySpan<char> span) {
    Asserts.False(span.IsEmpty, "Span was empty");

    ulong previousAccumulator = 0;
    ulong accumulator = 0;
    for (var i = 0; i < span.Length; i++) {
      var c = span[i];

      uint digit = (uint) (span[i] - '0');
      if (digit > 9) {
        Asserts.Fail($"Unexpected character in number: {c}");
      }

      previousAccumulator = accumulator;
      accumulator = accumulator * 10 + digit;

      if (accumulator < previousAccumulator) {
        Asserts.Fail("Overflow when parsing number");
      }
    }

    return accumulator;
  }
}