namespace schema.binary.steps;

public sealed class ClearMemberBinaryStep : IBinaryStep {
  public required ISchemaValueMember Member { get; init; }
}