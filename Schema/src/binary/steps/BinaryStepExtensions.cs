using System.Collections.Generic;

namespace schema.binary.steps;

public static class BinaryStepExtensions {
  public static IReadOnlyList<IBinaryStep> ToBinarySteps(
      this IBinarySchemaContainer container) {
    var steps = new List<IBinaryStep>();
    foreach (var member in container.Members) {
      if (member is ISchemaValueMember { IsSkipped: true }) {
        continue;
      }

      List<IBinaryStep> memberSteps = new();

      if (member is ISchemaValueMember { AlignStart: { } alignStart }) {
        memberSteps.Add(new AlignStartBinaryStep { Attribute = alignStart });
      }

      memberSteps.Add(new MemberBinaryStep { Member = member });

      if (member is ISchemaValueMember valueMember) {
        if (valueMember.Offset is { } offset) {
          memberSteps = [
              new AtPositionBinarySteps {
                  Offset = offset,
                  ValidOffsetChildren = memberSteps,
                  InvalidValidOffsetChildren = [
                      new ClearMemberBinaryStep { Member = valueMember }
                  ]
              }
          ];
        }

        if (valueMember.Endianness != null) {
          memberSteps = [
              new MemberEndiannessBinaryStep {
                  Endianness = valueMember.Endianness.Value,
                  Children = memberSteps
              }
          ];
        }

        if (valueMember.IfBoolean is { } ifBoolean) {
          memberSteps = [
              new IfBooleanBinarySteps {
                  Attribute = ifBoolean,
                  TrueChildren = memberSteps,
                  FalseChildren = [
                      new ClearMemberBinaryStep { Member = valueMember }
                  ]
              }
          ];
        }
      }

      steps.AddRange(memberSteps);
    }

    var containerEndianness = container.Endianness;
    if (containerEndianness != null) {
      steps = [
          new ContainerEndiannessBinaryStep {
              Endianness = containerEndianness.Value, Children = steps
          }
      ];
    }

    if (container.LocalPositions) {
      steps = [new LocalSpaceBinaryStep { Children = steps }];
    }

    if (container.AlignEnd != null) {
      steps.Add(new AlignEndBinaryStep { Attribute = container.AlignEnd });
    }

    return steps;
  }
}