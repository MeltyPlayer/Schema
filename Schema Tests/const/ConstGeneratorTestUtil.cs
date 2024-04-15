using System;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using NUnit.Framework;

using schema.@const;
using schema.util.asserts;

#pragma warning disable CS8604


namespace schema.binary {
  internal static class ConstGeneratorTestUtil {
    public static CSharpCompilation Compilation =
        CSharpCompilation
            .Create("test")
            .AddReferences(
                ((string) AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
                .Split(Path.PathSeparator)
                .Select(path => MetadataReference.CreateFromFile(path)));

    public static void AssertGenerated(string src, string expected) {
      var syntaxTree = CSharpSyntaxTree.ParseText(src);
      var compilation = BinarySchemaTestUtil.Compilation.Clone()
                                            .AddSyntaxTrees(syntaxTree);

      var semanticModel = compilation.GetSemanticModel(syntaxTree);

      var structures = syntaxTree
                       .GetRoot()
                       .DescendantTokens()
                       .Where(t => t is {
                           Text: "GenerateConst",
                           Parent.Parent: AttributeSyntax
                       })
                       .Select(t => t.Parent?.Parent as AttributeSyntax)
                       .Select(attributeSyntax => {
                                 var attributeListSyntax
                                     = Asserts.AsA<AttributeListSyntax>(
                                         attributeSyntax.Parent);
                                 var declarationSyntax
                                     = Asserts.AsA<TypeDeclarationSyntax>(
                                         attributeListSyntax.Parent);

                                 var symbol
                                     = semanticModel
                                         .GetDeclaredSymbol(declarationSyntax);
                                 var namedTypeSymbol
                                     = symbol as INamedTypeSymbol;

                                 return namedTypeSymbol;
                               })
                       .ToArray();

      var synbol = structures.First();

      Assert.True(new ConstTypeGenerator().Generate(synbol, out var actual));

      Assert.AreEqual(expected, actual.ReplaceLineEndings());
    }
  }
}