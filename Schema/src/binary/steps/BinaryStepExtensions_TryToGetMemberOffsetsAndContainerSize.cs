using System.Collections.Generic;

namespace schema.binary.steps;

public static partial class BinaryStepExtensions {
  public static void TryToGetMemberOffsetsAndContainerSize(
      this IReadOnlyList<IBinaryStep> steps,
      out IReadOnlyList<(ISchemaMember valueMember, int offset)>
          outValueMemberOffsets,
      out int? outContainerSize) {
    var valueMemberOffsets
        = new List<(ISchemaMember valueMember, int offset)>();
    var containerSize = 0;

    outContainerSize = null;

    foreach (var step in steps) {
      switch (step) {
        case AlignStartBinaryStep
             or AlignEndBinaryStep
             or IfBooleanBinarySteps: {
          outValueMemberOffsets = valueMemberOffsets;
          return;
        }
        case AtPositionBinarySteps
             or ClearMemberBinaryStep
             or ContainerEndiannessBinaryStep
             or LocalSpaceBinaryStep
             or MemberEndiannessBinaryStep: {
          // (Doesn't impact size, skip.)          
          break;
        }
        case MemberBinaryStep memberBinaryStep: {
          valueMemberOffsets.Add((memberBinaryStep.Member, containerSize));

          if (memberBinaryStep.Member is not ISchemaValueMember valueMember) {
            outValueMemberOffsets = valueMemberOffsets;
            return;
          }

          if (!SizeUtil.TryGetSizeOfType(valueMember.MemberType,
                                         out var memberSize)) {
            outValueMemberOffsets = valueMemberOffsets;
            return;
          }

          containerSize += memberSize;
          break;
        }
      }
    }

    outContainerSize = containerSize;
    outValueMemberOffsets = valueMemberOffsets;
  }
}