using System.Collections.Generic;

namespace schema.binary.steps;

public interface IGroupBinaryStep : IBinaryStep {
  IReadOnlyList<IBinaryStep> Children { get; }
}