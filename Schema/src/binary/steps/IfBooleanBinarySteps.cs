using System.Collections.Generic;

using schema.binary.attributes;

namespace schema.binary.steps;

public sealed class IfBooleanBinarySteps : IGroupBinaryStep {
  public required IIfBooleanAttribute Attribute { get; init; }
  public required IReadOnlyList<IBinaryStep> Children { get; init; }
}