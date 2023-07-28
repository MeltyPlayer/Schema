using System.Linq;

using schema.binary.attributes;

namespace schema.binary.dependencies {
  public static class DependencyExtensions {
    public static bool DependsOnSequenceImports(
        this IBinarySchemaStructure structure)
      => structure
         .Members
         .OfType<ISchemaValueMember>()
         .Any(
             member => member.MemberType is ISequenceMemberType {
                 LengthSourceType: not SequenceLengthSourceType
                     .UNTIL_END_OF_STREAM,
                 SequenceTypeInfo: { IsLengthConst: false },
             });

    public static bool DependsOnSystemText(
        this IBinarySchemaStructure structure)
      => structure
         .Members
         .OfType<ISchemaValueMember>()
         .Any(
             member => member.MemberType is IStringType {
                 EncodingType: not StringEncodingType.ASCII,
             });

    public static bool DependsOnCollectionsImports(
        this IBinarySchemaStructure structure)
      => structure
         .Members
         .OfType<ISchemaValueMember>()
         .Any(
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