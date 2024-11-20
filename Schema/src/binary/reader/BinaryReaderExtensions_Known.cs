using System;
using System.Numerics;
using System.Runtime.CompilerServices;

using CommunityToolkit.HighPerformance;

namespace schema.binary;

public static partial class BinaryReaderExtensions {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe T ReadFloatArrayStruct_<T>(this IBinaryReader br)
      where T : unmanaged {
    T value;

    T* ptr = &value;
    var span = new Span<T>(ptr, 1);
    br.ReadSingles(span.Cast<T, float>());

    return value;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 ReadVector2(this IBinaryReader br)
    => br.ReadFloatArrayStruct_<Vector2>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 ReadVector3(this IBinaryReader br)
    => br.ReadFloatArrayStruct_<Vector3>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 ReadVector4(this IBinaryReader br)
    => br.ReadFloatArrayStruct_<Vector4>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 ReadMatrix3x2(this IBinaryReader br)
    => br.ReadFloatArrayStruct_<Matrix3x2>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 ReadMatrix4x4(this IBinaryReader br)
    => br.ReadFloatArrayStruct_<Matrix4x4>();
}