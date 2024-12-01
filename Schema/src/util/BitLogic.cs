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
    public static double GetFixedPointDouble(uint x,
                                             byte signBits,
                                             byte integerBits,
                                             byte fractionBits) {
      double doubleValue;
      if (signBits == 0) {
        doubleValue = x;
      } else {
        var signMask = 1 << (integerBits + fractionBits);
        if ((x & signMask) != 0) {
          doubleValue = (double) (x | ~(signMask - 1));
        } else {
          doubleValue = x;
        }
      }

      return doubleValue * Math.Pow(.5, fractionBits);
    }
  }
}