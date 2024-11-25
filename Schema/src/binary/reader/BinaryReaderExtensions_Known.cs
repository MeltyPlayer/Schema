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
  private static void ReadFloatArrayStructs_<T>(this IBinaryReader br,
                                                Span<T> dst)
      where T : unmanaged
    => br.ReadSingles(dst.Cast<T, float>());


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 ReadVector2(this IBinaryReader br)
    => br.ReadFloatArrayStruct_<Vector2>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void ReadVector2s(this IBinaryReader br, Span<Vector2> dst)
    => br.ReadFloatArrayStructs_(dst);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2[] ReadVector2s(this IBinaryReader br, int count) {
    var array = new Vector2[count];
    br.ReadVector2s(array);
    return array;
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 ReadVector3(this IBinaryReader br)
    => br.ReadFloatArrayStruct_<Vector3>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void ReadVector3s(this IBinaryReader br, Span<Vector3> dst)
    => br.ReadFloatArrayStructs_(dst);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3[] ReadVector3s(this IBinaryReader br, int count) {
    var array = new Vector3[count];
    br.ReadVector3s(array);
    return array;
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 ReadVector4(this IBinaryReader br)
    => br.ReadFloatArrayStruct_<Vector4>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void ReadVector4s(this IBinaryReader br, Span<Vector4> dst)
    => br.ReadFloatArrayStructs_(dst);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4[] ReadVector4s(this IBinaryReader br, int count) {
    var array = new Vector4[count];
    br.ReadVector4s(array);
    return array;
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 ReadMatrix3x2(this IBinaryReader br)
    => br.ReadFloatArrayStruct_<Matrix3x2>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void ReadMatrix3x2s(this IBinaryReader br, Span<Matrix3x2> dst)
    => br.ReadFloatArrayStructs_(dst);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2[] ReadMatrix3x2s(this IBinaryReader br, int count) {
    var array = new Matrix3x2[count];
    br.ReadMatrix3x2s(array);
    return array;
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 ReadMatrix4x4(this IBinaryReader br)
    => br.ReadFloatArrayStruct_<Matrix4x4>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void ReadMatrix4x4s(this IBinaryReader br, Span<Matrix4x4> dst)
    => br.ReadFloatArrayStructs_(dst);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4[] ReadMatrix4x4s(this IBinaryReader br, int count) {
    var array = new Matrix4x4[count];
    br.ReadMatrix4x4s(array);
    return array;
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion ReadQuaternion(this IBinaryReader br)
    => br.ReadFloatArrayStruct_<Quaternion>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void ReadQuaternions(this IBinaryReader br,
                                     Span<Quaternion> dst)
    => br.ReadFloatArrayStructs_(dst);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion[] ReadQuaternions(this IBinaryReader br, int count) {
    var array = new Quaternion[count];
    br.ReadQuaternions(array);
    return array;
  }
}