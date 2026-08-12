namespace schema.binary.steps;

public sealed class BlitBinaryStep : IBinaryStep {
  public string MemberName { get; set; }
  public int ElementSizeInBytes { get; set; }
  public int ElementCount { get; set; }
}