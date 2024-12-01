using System;

using schema.util;

namespace schema.binary;

public static partial class BinaryReaderExtensions {
  /// <summary>
  ///   Shamelessly stolen from:
  ///   https://github.com/scurest/apicula/blob/3d4e91e14045392a49c89e86dab8cb936225588c/src/util/fixed.rs#L5-L35
  /// </summary>
  public static double ReadDoubleFixedPoint(this IBinaryReader br,
                                            byte signBits,
                                            byte integerBits,
                                            byte fractionBits) {
    var totalBits = (uint) (signBits + integerBits + fractionBits);
    var totalBytes = (int) BitLogic.BytesNeededToContainBits(totalBits);

    var value = totalBytes switch {
        1 => br.ReadByte(),
        2 => br.ReadUInt16(),
        3 => br.ReadUInt24(),
        4 => br.ReadUInt32(),
    };

    double doubleValue;
    if (signBits == 0) {
      doubleValue = value;
    } else {
      var signMask = 1 << (integerBits + fractionBits);
      if ((value & signMask) != 0) {
        doubleValue = (double) (value | ~(signMask - 1));
      } else {
        doubleValue = value;
      }
    }

    return doubleValue * Math.Pow(.5, fractionBits);
  }
}