namespace schema.binary.types.matrix {
  [BinarySchema]
  public partial class Matrix4x4f : IBinaryConvertible {
    public float[] Values { get; } = new float[4 * 4];

    public float this[int row, int column] {
      get => this.Values[4 * row + column];
      set => this.Values[4 * row + column] = value;
    }
  }
}
