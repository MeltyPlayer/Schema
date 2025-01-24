using System.Runtime.CompilerServices;

namespace schema.binary;

public static partial class BinaryReaderExtensions {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe byte[] ReadToEnd(this IBinaryReader br)
    => br.ReadBytes(br.Length - br.Position);
}