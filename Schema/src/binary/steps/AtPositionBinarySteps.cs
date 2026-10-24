using System.Collections.Generic;

namespace schema.binary.steps;

public sealed class AtPositionBinarySteps : IGroupBinaryStep {
  public required IOffset Offset { get; init; }
  public required IReadOnlyList<IBinaryStep> Children { get; init; }
}