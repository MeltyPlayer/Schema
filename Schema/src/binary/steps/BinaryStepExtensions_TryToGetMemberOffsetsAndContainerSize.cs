using System.Collections.Generic;

using schema.binary.attributes;
using schema.binary.parser;

namespace schema.binary.steps;

public interface IStepOffset {
  int? Offset { get; }
}

public class MemberOffset : IStepOffset {
  public required ISchemaMember Member { get; init; }
  public required int? Offset { get; init; }

  public override string ToString()
    => $"{this.Member.Name}: {(this.Offset == null ? "(indeterminate)" : $"0x{this.Offset:X}")}";
}

public class LabelledOffset : IStepOffset {
  public required string Text { get; init; }
  public required int? Offset { get; init; }

  public override string ToString()
    => $"{this.Text}: {(this.Offset == null ? "(indeterminate)" : $"0x{this.Offset:X}")}";
}

public static partial class BinaryStepExtensions {

  public static void TryToGetMemberOffsetsAndContainerSize(
      this IReadOnlyList<IBinaryStep> steps,
      out IReadOnlyList<IStepOffset> outStepOffsets,
      out int? containerSize) {
    var stepOffsets = new List<IStepOffset>();
    containerSize = 0;

    TryToGetStepOffsetsAndContainerSizeImpl_(
        steps,
        stepOffsets,
        ref containerSize);

    outStepOffsets = stepOffsets;
  }

  private static void TryToGetStepOffsetsAndContainerSizeImpl_(
      IReadOnlyList<IBinaryStep> steps,
      List<IStepOffset> stepOffsets,
      ref int? containerSize,
      bool onlyFirstIsDeterminate = false) {
    foreach (var step in steps) {
      switch (step) {
        case IfBooleanBinarySteps ifBooleanStep: {
          var ifBooleanAttribute = ifBooleanStep.Attribute;
          if (ifBooleanAttribute.SourceType is IfBooleanSourceType
                  .IMMEDIATE_VALUE) {
            stepOffsets.Add(new LabelledOffset {
                Text = "IfBoolean (Immediate)",
                Offset = containerSize
            });
            containerSize
                += SizeUtil.GetSize(ifBooleanAttribute.ImmediateBooleanType);
          }

          TryToGetStepOffsetsAndContainerSizeImpl_(
              ifBooleanStep.TrueChildren,
              stepOffsets,
              ref containerSize,
              true);

          containerSize = null;
          break;
        }
        case AlignStartBinaryStep or AlignEndBinaryStep: {
          stepOffsets.Add(new LabelledOffset {
              Text = "Align", 
              Offset = containerSize
          });
          containerSize = null;
          break;
        }
        case ClearMemberBinaryStep or LocalSpaceBinaryStep: {
          // (Doesn't impact size, skip.)          
          break;
        }
        case ContainerEndiannessBinaryStep containerEndiannessBinaryStep: {
          TryToGetStepOffsetsAndContainerSizeImpl_(
              containerEndiannessBinaryStep.Children,
              stepOffsets,
              ref containerSize);
          break;
        }
        case MemberEndiannessBinaryStep memberEndiannessBinaryStep: {
          TryToGetStepOffsetsAndContainerSizeImpl_(
              memberEndiannessBinaryStep.Children,
              stepOffsets,
              ref containerSize);
          break;
        }
        case MemberBinaryStep memberBinaryStep: {
          var valueMember = memberBinaryStep.Member as ISchemaValueMember;

          if (valueMember?.MemberType is IStringType {
                  LengthSourceType: StringLengthSourceType.IMMEDIATE_VALUE,
                  ImmediateLengthType: var immediateStringLengthType,
              }) {
            stepOffsets.Add(new LabelledOffset {
                Text = $"{valueMember.Name}.Length",
                Offset = containerSize
            });
            containerSize += SizeUtil.GetSize(immediateStringLengthType);
          }
          if (valueMember?.MemberType is ISequenceMemberType {
                  LengthSourceType: SequenceLengthSourceType.IMMEDIATE_VALUE,
                  ImmediateLengthType: var immediateSequenceLengthType,
              }) {
            stepOffsets.Add(new LabelledOffset {
                Text = $"{valueMember.Name}.Length",
                Offset = containerSize
            });
            containerSize += SizeUtil.GetSize(immediateSequenceLengthType);
          }

          stepOffsets.Add(new MemberOffset {
              Member = memberBinaryStep.Member,
              Offset = containerSize
          });

          if (valueMember != null) {
            if (SizeUtil.TryGetSizeOfType(valueMember.MemberType!,
                                          out var memberSize)) {
              containerSize += memberSize;
            } else {
              containerSize = null;
            }
          } else {
            containerSize = null;
          }

          break;
        }
      }

      if (onlyFirstIsDeterminate) {
        containerSize = null;
      }
    }
  }
}