using System;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using NUnit.Framework;

using schema.binary;
using schema.readOnly;
using schema.util.asserts;

#pragma warning disable CS8604


namespace schema.autoInterface;

internal static class InterfaceGeneratorTestUtil {
  public static void AssertGenerated(string src, params string[] expected) {
    var syntaxTree = CSharpSyntaxTree.ParseText(src);
    var compilation = BinarySchemaTestUtil.Compilation.Clone()
                                          .AddSyntaxTrees(syntaxTree);

    var semanticModel = compilation.GetSemanticModel(syntaxTree);

    var actual = syntaxTree
                 .GetRoot()
                 .DescendantTokens()
                 .Where(t => t is {
                     Text: "GenerateInterface",
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

                   return (namedTypeSymbol, declarationSyntax);
                 })
                 .Select(symbolAndSyntax
                             => new AutoInterfaceTypeGenerator()
                                .GenerateSourceForNamedType(
                                    symbolAndSyntax.namedTypeSymbol,
                                    semanticModel,
                                    symbolAndSyntax.declarationSyntax)
                                .ReplaceLineEndings());

    CollectionAssert.AreEqual(expected, actual);
  }
}