using schema.binary.attributes;

namespace schema.binary.steps;

public sealed class AlignEndBinaryStep : IBinaryStep {
  public required AlignEndAttribute Attribute { get; init; }
}