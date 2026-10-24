using System.Collections.Generic;

using schema.binary.attributes;

namespace schema.binary.steps;

public sealed class IfBooleanBinarySteps : IBinaryStep {
  public required IIfBooleanAttribute Attribute { get; init; }
  public required ISchemaValueMember Member { get; init; }

  public required IReadOnlyList<IBinaryStep> TrueChildren { get; init; }
  public required IReadOnlyList<IBinaryStep> FalseChildren { get; init; }
}