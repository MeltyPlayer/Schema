﻿using System;
using System.Runtime.CompilerServices;


namespace schema.binary;

public sealed partial class SchemaBinaryReader {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte ConvertByte_(ReadOnlySpan<byte> buffer, int i)
    => buffer[i];

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static sbyte ConvertSByte_(ReadOnlySpan<byte> buffer, int i)
    => (sbyte) buffer[i];

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int ConvertInt24_(ReadOnlySpan<byte> buffer, int i) {
    var value = (buffer[3 * i + 2] << 16) |
                (buffer[3 * i + 1] << 8) |
                buffer[3 * i];

    const int bitMask = -16777216;

    // Stolen from https://github.com/GridProtectionAlliance/gsf/blob/master/Source/Libraries/GSF.Core.Shared/Int24.cs
    // Check bit 23, the sign bit in a signed 24-bit integer
    if ((value & 0x00800000) > 0) {
      // If the sign-bit is set, this number will be negative - set all high-byte bits (keeps 32-bit number in 24-bit range)
      value |= bitMask;
    } else {
      // If the sign-bit is not set, this number will be positive - clear all high-byte bits (keeps 32-bit number in 24-bit range)
      value &= ~bitMask;
    }

    return value;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static uint ConvertUInt24_(ReadOnlySpan<byte> buffer, int i)
    => (uint) ((buffer[3 * i + 2] << 16) |
               (buffer[3 * i + 1] << 8) |
               buffer[3 * i]);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float ConvertHalf_(ushort value)
    => Half.ToHalf(value);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float ConvertSn8_(ReadOnlySpan<byte> buffer, int i)
    => SchemaBinaryReader.ConvertSByte_(buffer, i) / (255f / 2);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float ConvertUn8_(ReadOnlySpan<byte> buffer, int i)
    => SchemaBinaryReader.ConvertByte_(buffer, i) / 255f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float ConvertSn16_(short value) => value / (65535f / 2);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float ConvertUn16_(ushort value) => value / 65535f;
}