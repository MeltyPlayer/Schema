using System.Linq;

using schema.binary.attributes;


namespace schema.binary.dependencies;

public static class DependencyExtensions {
  public static bool DependsOnSequenceImports(
      this IBinarySchemaContainer container)
    => container
       .Members
       .OfType<ISchemaValueMember>()
       .Any(
           member => member.MemberType is ISequenceMemberType {
               LengthSourceType: not SequenceLengthSourceType
                   .UNTIL_END_OF_STREAM,
               SequenceTypeInfo: {IsLengthConst: false},
           });

  public static bool DependsOnSchemaAttributes(
      this IBinarySchemaContainer container)
    => container
       .Members
       .OfType<ISchemaValueMember>()
       .Any(
           member => member.MemberType is IStringType {
               EncodingType: not StringEncodingType.ASCII,
           });


  public static bool DependsOnSchemaUtilAsserts(
      this IBinarySchemaContainer container)
    => container
       .Members
       .OfType<ISchemaValueMember>()
       .Any(
           member
               => member.MemberType is
                   IPrimitiveMemberType {
                       LengthOfStringMembers.Length: > 1
                   } or
                   IPrimitiveMemberType {
                       LengthOfSequenceMembers.Length: > 1
                   });


  public static bool DependsOnSystemThreadingTasks(
      this IBinarySchemaContainer container)
    => container
       .Members
       .OfType<ISchemaValueMember>()
       .Any(member => member.MemberType is IPrimitiveMemberType {
           PointerToAttribute: {NullValue: { }}
       });

  public static bool DependsOnCollectionsImports(
      this IBinarySchemaContainer container)
    => container
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