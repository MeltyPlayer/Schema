using System;
using System.Runtime.CompilerServices;

using schema.util.asserts;


namespace schema.text.reader;

public sealed partial class SchemaTextReader {
  private byte ConvertHexByte_(ReadOnlySpan<char> span)
    => (byte) ParseHexUlongLessThan_(span, byte.MaxValue);

  private sbyte ConvertHexSByte_(ReadOnlySpan<char> span)
    => (sbyte) ParseHexLongWithinRange(span, sbyte.MinValue, sbyte.MaxValue);

  private short ConvertHexInt16_(ReadOnlySpan<char> span)
    => (short) ParseHexLongWithinRange(span, short.MinValue, short.MaxValue);

  private ushort ConvertHexUInt16_(ReadOnlySpan<char> span)
    => (ushort) ParseHexUlongLessThan_(span, ushort.MaxValue);

  private int ConvertHexInt32_(ReadOnlySpan<char> span)
    => (int) ParseHexLongWithinRange(span, int.MinValue, int.MaxValue);

  private uint ConvertHexUInt32_(ReadOnlySpan<char> span)
    => (uint) ParseHexUlongLessThan_(span, uint.MaxValue);

  private long ConvertHexInt64_(ReadOnlySpan<char> span)
    => ParseHexLongWithinRange(span, long.MinValue, long.MaxValue);

  private ulong ConvertHexUInt64_(ReadOnlySpan<char> span) => ParseHexUlong_(span);

  public static long ParseHexLongWithinRange(
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

    if (span.Length - i >= 2 &&
        span[i] == '0' &&
        (span[i + 1] == 'x' || span[i + 1] == 'X')) {
      i += 2;
      if (span.Length == i) {
        Asserts.Fail("Span only contained hex prefix");
      }
    }

    return negative
        ? unchecked(-(long) ParseHexUlongLessThan_(
            span[i..],
            unchecked((ulong) -minValue)))
        : (long) ParseHexUlongLessThan_(span[i..], (ulong) maxValue);
  }

  public static ulong ParseHexUlongLessThan_(
      ReadOnlySpan<char> span,
      ulong maxValue) {
    var value = ParseHexUlong_(span);
    if (value > maxValue) {
      Asserts.Fail($"Expected to read value less than or equal to: {maxValue}");
    }

    return value;
  }

  public static ulong ParseHexUlong_(ReadOnlySpan<char> span) {
    Asserts.False(span.IsEmpty, "Span was empty");

    ulong accumulator = 0;
    for (var i = 0; i < span.Length; i++) {
      var c = span[i];
      uint digit = ParseHexDigit_(c);

      if (digit > 15) {
        Asserts.Fail($"Unexpected character in hex number: {c}");
      }

      // Check before shifting left by 4 bits (equivalent to * 16)
      if (accumulator > (ulong.MaxValue >> 4)) {
        Asserts.Fail("Overflow when parsing hex number");
      }

      accumulator = (accumulator << 4) | digit;
    }

    return accumulator;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static uint ParseHexDigit_(char c) {
    uint digit = (uint) (c - '0');
    if (digit <= 9) {
      return digit;
    }

    // Bitwise OR with 0x20 folds uppercase 'A'-'F' into lowercase 'a'-'f'
    uint letter = (uint) ((c | 0x20) - 'a');
    if (letter <= 5) {
      return letter + 10;
    }

    return uint.MaxValue; // Invalid character
  }
}