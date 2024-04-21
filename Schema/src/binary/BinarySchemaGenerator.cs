using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using schema.binary.attributes;
using schema.binary.parser;
using schema.binary.text;
using schema.util.generators;
using schema.util.syntax;
using schema.util.types;

namespace schema.binary {
  [Generator(LanguageNames.CSharp)]
  public class BinarySchemaGenerator
      : BMappedNamedTypesWithAttributeGenerator<BinarySchemaAttribute,
          IBinarySchemaContainer> {
    private readonly BinarySchemaContainerParser parser_ = new();

    private readonly BinarySchemaReaderGenerator readerImpl_ = new();
    private readonly BinarySchemaWriterGenerator writerImpl_ = new();

    public override bool TryToMap(
        TypeDeclarationSyntax syntax,
        INamedTypeSymbol typeSymbol,
        out IBinarySchemaContainer mapped) {
      mapped = default;
      if (!syntax.IsPartial()) {
        return false;
      }

      mapped = this.parser_.ParseContainer(typeSymbol);
      return true;
    }

    public override void PreprocessAllMapped(
        IReadOnlyDictionary<INamedTypeSymbol, IBinarySchemaContainer>
            containerByNamedTypeSymbol) {
      // Hooks up size of dependencies.
      {
        var sizeOfMemberInBytesDependencyFixer =
            new WSizeOfMemberInBytesDependencyFixer();
        foreach (var container in containerByNamedTypeSymbol.Values) {
          foreach (var member in
                   container.Members.OfType<ISchemaValueMember>()) {
            if (member.MemberType is IPrimitiveMemberType
                primitiveMemberType) {
              if (primitiveMemberType.AccessChainToSizeOf != null) {
                sizeOfMemberInBytesDependencyFixer.AddDependenciesForContainer(
                    containerByNamedTypeSymbol,
                    primitiveMemberType.AccessChainToSizeOf);
              }

              var pointerToAttribute = primitiveMemberType.PointerToAttribute;
              if (pointerToAttribute != null) {
                sizeOfMemberInBytesDependencyFixer.AddDependenciesForContainer(
                    containerByNamedTypeSymbol,
                    pointerToAttribute.AccessChainToOtherMember);
              }
            }
          }
        }
      }
    }

    public override void PreprocessCompilation(Compilation compilation) {
      MemberReferenceUtil.PopulateBinaryTypes(compilation);
    }

    public override IEnumerable<(string fileName, string source)>
        GenerateSourcesForMappedNamedType(IBinarySchemaContainer container) {
      var containerTypeV2 = TypeV2.FromSymbol(container.TypeSymbol);
      if (containerTypeV2.Implements<IBinaryDeserializable>() &&
          container.TypeSymbol.MemberNames.All(member => member != "Read")) {
        var readerCode = this.readerImpl_.Generate(container);
        yield return ($"{containerTypeV2.FullyQualifiedName}_reader.g",
                      readerCode);
      }

      if (containerTypeV2.Implements<IBinarySerializable>() &&
          container.TypeSymbol.MemberNames.All(
              member => member != "Write")) {
        var writerCode = this.writerImpl_.Generate(container);
        yield return ($"{containerTypeV2.FullyQualifiedName}_writer.g",
                      writerCode);
      }
    }
  }
}