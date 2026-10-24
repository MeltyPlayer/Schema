namespace schema.binary.steps;

public sealed class BlitBinaryStep : IBinaryStep {
  public required string MemberName { get; init; }
  public required int ElementSizeInBytes { get; init; }
  public required int ElementCount { get; init; }
}