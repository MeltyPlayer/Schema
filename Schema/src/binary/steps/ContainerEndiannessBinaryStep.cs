using System.Collections.Generic;

namespace schema.binary.steps;

public sealed class ContainerEndiannessBinaryStep : IGroupBinaryStep {
  public required Endianness Endianness { get; init; }
  public required IReadOnlyList<IBinaryStep> Children { get; init; }
}