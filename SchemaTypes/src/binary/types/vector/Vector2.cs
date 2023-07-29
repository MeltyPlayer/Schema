using System;


namespace schema.binary.types.vector {
  public abstract class BVector2<T> {
    public T X { get; set; }
    public T Y { get; set; }

    public T this[int index] {
      get => index switch {
          0 => X,
          1 => Y,
      };
      set {
        switch (index) {
          case 0: {
            this.X = value;
            break;
          }
          case 1: {
            this.Y = value;
            break;
          }
          default: throw new ArgumentOutOfRangeException();
        }
      }
    }

    public void Set(T x, T y) {
      this.X = x;
      this.Y = y;
    }

    public override string ToString() => $"{{{this.X}, {this.Y}}}";
  }

  [BinarySchema]
  public sealed partial class Vector2f : BVector2<float>, IBinaryConvertible { }

  [BinarySchema]
  public sealed partial class Vector2i : BVector2<int>, IBinaryConvertible { }

  [BinarySchema]
  public sealed partial class Vector2s : BVector2<short>, IBinaryConvertible { }
}