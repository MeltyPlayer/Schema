using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using NUnit.Framework;

using schema.binary.attributes.size;
using schema.defaultinterface;

#pragma warning disable CS8604


namespace schema.binary {
  internal static class DefaultInterfaceMethodsTestUtil {
    public static DefaultInterfaceMethodsData ParseFirst(string src)
      => ParseAll(src).First();

    public static IReadOnlyList<DefaultInterfaceMethodsData> ParseAll(
        string src) {
      var syntaxTree = CSharpSyntaxTree.ParseText(src);

      var references =
          ((string) AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
          .Split(Path.PathSeparator)
          .Select(path => MetadataReference.CreateFromFile(path));

      var compilation =
          CSharpCompilation.Create("test")
                           .AddReferences(references)
                           .AddSyntaxTrees(syntaxTree);
      var semanticModel = compilation.GetSemanticModel(syntaxTree);

      var structures = syntaxTree
                       .GetRoot()
                       .DescendantTokens()
                       .Where(t => {
                         if (t.Text == "IncludeDefaultInterfaceMethods" &&
                             t.Parent
                              ?.Parent is AttributeSyntax) {
                           return true;
                         }

                         return false;
                       })
                       .Select(t => t.Parent?.Parent as AttributeSyntax)
                       .Select(attributeSyntax => {
                         var attributeSpan = attributeSyntax!.FullSpan;

                         var classIndex =
                             src.IndexOf("class",
                                         attributeSpan.Start +
                                         attributeSpan.Length);
                         var structIndex = src.IndexOf("struct",
                           attributeSpan.Start +
                           attributeSpan.Length);

                         if (classIndex == -1) {
                           classIndex = int.MaxValue;
                         }

                         if (structIndex == -1) {
                           structIndex = int.MaxValue;
                         }

                         var typeSpecifierIndex =
                             Math.Min(classIndex, structIndex);
                         var typeNameIndex =
                             src.IndexOf(' ', typeSpecifierIndex) + 1;
                         var typeNameLength =
                             src.IndexOf(' ', typeNameIndex) - typeNameIndex;

                         var typeName =
                             src.Substring(typeNameIndex, typeNameLength);
                         var angleBracketIndex = typeName.IndexOf('<');
                         if (angleBracketIndex > -1) {
                           typeName = typeName.Substring(0, angleBracketIndex);
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
                                                  .Parent;

                         var symbol = semanticModel.GetDeclaredSymbol(typeNode);
                         var namedTypeSymbol = symbol as INamedTypeSymbol;

                         return new IncludeDefaultInterfaceMethodsParser()
                             .ParseStructure(
                                 namedTypeSymbol);
                       })
                       .ToArray();

      return structures;
    }

    public static void AssertGenerated(string src,
                                       string expected) {
      var data = DefaultInterfaceMethodsTestUtil.ParseFirst(src);

      var actual = new IncludeDefaultInterfaceMethodsWriter().Generate(data);

      Assert.AreEqual(expected, actual.ReplaceLineEndings());
    }
  }
}