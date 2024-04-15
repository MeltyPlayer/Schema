using System;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using NUnit.Framework;

using schema.@const;

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
                                 var attributeSpan = attributeSyntax!.FullSpan;

                                 var classIndex =
                                     src.IndexOf("class",
                                                 attributeSpan.Start +
                                                 attributeSpan.Length);
                                 var classNameIndex
                                     = src.IndexOf(' ', classIndex) + 1;
                                 var classNameLength =
                                     src.IndexOf(' ', classNameIndex) -
                                     classNameIndex;

                                 var typeName =
                                     src.Substring(
                                         classNameIndex,
                                         classNameLength);
                                 var angleBracketIndex = typeName.IndexOf('<');
                                 if (angleBracketIndex > -1) {
                                   typeName
                                       = typeName.Substring(
                                           0,
                                           angleBracketIndex);
                                 }

                                 var typeNode = syntaxTree.GetRoot()
                                     .DescendantTokens()
                                     .Single(t =>
                                                 t.Text ==
                                                 typeName &&
                                                 t.Parent is
                                                     ClassDeclarationSyntax
                                                     or StructDeclarationSyntax
                                     )
                                     .Parent as TypeDeclarationSyntax;

                                 var symbol
                                     = semanticModel
                                         .GetDeclaredSymbol(typeNode);
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