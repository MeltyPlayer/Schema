using System.Linq;

namespace schema.binary.dependencies {
  public static class DependencyExtensions {
    public static bool DependsOnSequenceImports(
        this IBinarySchemaStructure structure)
      => structure.Members.Any(
          member => member.MemberType is ISequenceMemberType {
              LengthSourceType: not SequenceLengthSourceType
                  .UNTIL_END_OF_STREAM,
              SequenceTypeInfo: { IsLengthConst: false },
          });

    public static bool DependsOnCollectionsImports(
        this IBinarySchemaStructure structure)
      => structure.Members.Any(
          member => member is {
              MemberType: ISequenceMemberType {
                  LengthSourceType: SequenceLengthSourceType
                      .UNTIL_END_OF_STREAM
              }
          } or {
              MemberType: ISequenceMemberType,
              IfBoolean: { },
          });
  }
}