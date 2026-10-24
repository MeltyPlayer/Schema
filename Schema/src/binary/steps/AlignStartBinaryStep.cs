using schema.binary.attributes;

namespace schema.binary.steps;

public sealed class AlignStartBinaryStep : IBinaryStep {
  public required AlignStartAttribute Attribute { get; init; }
}