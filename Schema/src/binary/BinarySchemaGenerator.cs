using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using schema.binary.attributes;
using schema.binary.text;
using schema.util.generators;
using schema.util.symbols;
using schema.util.syntax;
using schema.util.types;

namespace schema.binary {
  [Generator(LanguageNames.CSharp)]
  internal class BinarySchemaGenerator
      : BNamedTypeSecondaryGenerator<IBinarySchemaContainer> {
    private readonly BinarySchemaContainerParser parser_ = new();

    private readonly BinarySchemaReaderGenerator readerImpl_ = new();
    private readonly BinarySchemaWriterGenerator writerImpl_ = new();

    internal override bool TryToMapToSecondary(
        TypeDeclarationSyntax syntax,
        INamedTypeSymbol typeSymbol,
        out IBinarySchemaContainer secondary) {
      secondary = default;
      if (!typeSymbol.HasAttribute<BinarySchemaAttribute>()) {
        return false;
      }

      if (!syntax.IsPartial()) {
        return false;
      }

      secondary = this.parser_.ParseContainer(typeSymbol);
      return true;
    }

    internal override void PreprocessSecondaries(
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

    internal override void Generate(
        IBinarySchemaContainer container,
        ISourceFileDictionary sourceFileDictionary) {
      var containerTypeV2 = TypeV2.FromSymbol(container.TypeSymbol);
      if (containerTypeV2.Implements<IBinaryDeserializable>() &&
          container.TypeSymbol.MemberNames.All(member => member != "Read")) {
        var readerCode = this.readerImpl_.Generate(container);
        sourceFileDictionary.Add(
            $"{containerTypeV2.FullyQualifiedName}_reader.g",
            readerCode);
      }

      if (containerTypeV2.Implements<IBinarySerializable>() &&
          container.TypeSymbol.MemberNames.All(
              member => member != "Write")) {
        var writerCode = this.writerImpl_.Generate(container);
        sourceFileDictionary.Add(
            $"{containerTypeV2.FullyQualifiedName}_writer.g",
            writerCode);
      }
    }
  }
}