using System.Collections.Generic;

namespace schema.binary.steps;

public sealed class LocalSpaceBinaryStep : IGroupBinaryStep {
  public required IReadOnlyList<IBinaryStep> Children { get; init; }
}