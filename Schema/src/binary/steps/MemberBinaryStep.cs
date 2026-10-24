namespace schema.binary.steps;

public sealed class MemberBinaryStep : IBinaryStep {
  public required ISchemaMember Member { get; init; }
}