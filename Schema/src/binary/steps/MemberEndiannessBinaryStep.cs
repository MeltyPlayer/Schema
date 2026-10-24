using System.Collections.Generic;

namespace schema.binary.steps;

public sealed class MemberEndiannessBinaryStep : IGroupBinaryStep {
  public required Endianness Endianness { get; init; }
  public required IReadOnlyList<IBinaryStep> Children { get; init; }
}