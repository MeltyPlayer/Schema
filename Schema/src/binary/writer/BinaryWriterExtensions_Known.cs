using System;
using System.Numerics;
using System.Runtime.CompilerServices;

using CommunityToolkit.HighPerformance;

namespace schema.binary;

public static partial class BinaryWriterExtensions {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void WriteFloatArrayStruct_<T>(
      this IBinaryWriter bw,
      in T value)
      where T : unmanaged {
    fixed (T* ptr = &value) {
      var span = new Span<T>(ptr, 1);
      bw.WriteSingles(span.Cast<T, float>());
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void WriteFloatArrayStructs_<T>(this IBinaryWriter bw,
                                                 ReadOnlySpan<T> dst)
      where T : unmanaged
    => bw.WriteSingles(dst.Cast<T, float>());


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void WriteVector2(this IBinaryWriter bw, in Vector2 value)
    => bw.WriteFloatArrayStruct_(value);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void WriteVector2s(this IBinaryWriter bw,
                                   ReadOnlySpan<Vector2> values)
    => bw.WriteFloatArrayStructs_(values);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void WriteVector3(this IBinaryWriter bw, in Vector3 value)
    => bw.WriteFloatArrayStruct_(value);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void WriteVector3s(this IBinaryWriter bw,
                                   ReadOnlySpan<Vector3> values)
    => bw.WriteFloatArrayStructs_(values);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void WriteVector4(this IBinaryWriter bw, in Vector4 value)
    => bw.WriteFloatArrayStruct_(value);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void WriteVector4s(this IBinaryWriter bw,
                                   ReadOnlySpan<Vector4> values)
    => bw.WriteFloatArrayStructs_(values);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void WriteMatrix3x2(this IBinaryWriter bw, in Matrix3x2 value)
    => bw.WriteFloatArrayStruct_(value);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void WriteMatrix3x2s(this IBinaryWriter bw,
                                     ReadOnlySpan<Matrix3x2> values)
    => bw.WriteFloatArrayStructs_(values);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void WriteMatrix4x4(this IBinaryWriter bw, in Matrix4x4 value)
    => bw.WriteFloatArrayStruct_(value);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void WriteMatrix4x4s(this IBinaryWriter bw,
                                     ReadOnlySpan<Matrix4x4> values)
    => bw.WriteFloatArrayStructs_(values);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void WriteQuaternion(this IBinaryWriter bw, in Quaternion value)
    => bw.WriteFloatArrayStruct_(value);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void WriteQuaternions(this IBinaryWriter bw,
                                      ReadOnlySpan<Quaternion> values)
    => bw.WriteFloatArrayStructs_(values);
}