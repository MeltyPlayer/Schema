using System.Runtime.CompilerServices;

namespace schema.util {
  public static class BitLogic {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint BytesNeededToContainBits(uint bits) {
      var floorBytes = bits >> 3;
      return (bits - (floorBytes << 3)) == 0 ? floorBytes : floorBytes + 1;
    }
  }
}