using System.Collections.Generic;

namespace schema.binary.steps;

public sealed class AtPositionBinarySteps : IBinaryStep {
  public required IOffset Offset { get; init; }
  public required IReadOnlyList<IBinaryStep> ValidOffsetChildren { get; init; }
  public required IReadOnlyList<IBinaryStep> InvalidValidOffsetChildren { get; init; }
}