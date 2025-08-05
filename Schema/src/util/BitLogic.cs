using System;
using System.Runtime.CompilerServices;

namespace schema.util {
  public static class BitLogic {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint BytesNeededToContainBits(uint bits) {
      var floorBytes = bits >> 3;
      return (bits - (floorBytes << 3)) == 0 ? floorBytes : floorBytes + 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint CreateMask(int bits)
      => bits >= 32 ? uint.MaxValue : (uint) ((1 << bits) - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ConvertFixedPointToSingle(uint x,
                                                  byte signBits,
                                                  byte integerBits,
                                                  byte fractionBits) {
      float floatValue;
      if (signBits == 0) {
        floatValue = x;
      } else {
        var signMask = 1 << (integerBits + fractionBits);
        if ((x & signMask) != 0) {
          floatValue = (x | ~(signMask - 1));
        } else {
          floatValue = x;
        }
      }

      return floatValue * (float) Math.Pow(.5f, fractionBits);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ConvertFixedPointToDouble(uint x,
                                                   byte signBits,
                                                   byte integerBits,
                                                   byte fractionBits) {
      double doubleValue;
      if (signBits == 0) {
        doubleValue = x;
      } else {
        var signMask = 1 << (integerBits + fractionBits);
        if ((x & signMask) != 0) {
          doubleValue = (x | ~(signMask - 1));
        } else {
          doubleValue = x;
        }
      }

      return doubleValue * Math.Pow(.5, fractionBits);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ConvertSingleToFixedPoint(float x,
                                                 byte signBits,
                                                 byte integerBits,
                                                 byte fractionBits) 
      => (uint) (x * Math.Pow(2, fractionBits)) &
         BitLogic.CreateMask(signBits + integerBits + fractionBits);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ConvertDoubleToFixedPoint(double x,
                                                 byte signBits,
                                                 byte integerBits,
                                                 byte fractionBits)
      => (uint) (x * Math.Pow(2, fractionBits)) & BitLogic.CreateMask(signBits + integerBits + fractionBits);
  }
}