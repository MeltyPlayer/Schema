﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using NUnit.Framework;

using schema.binary.attributes.size;
using schema.binary.text;

#pragma warning disable CS8604


namespace schema.binary {
  internal static class BinarySchemaTestUtil {
    public static IBinarySchemaStructure ParseFirst(string src)
      => ParseAll(src).First();

    public static IReadOnlyList<IBinarySchemaStructure> ParseAll(string src) {
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
                         if (t.Text == "BinarySchema" &&
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
                         var classNameIndex = src.IndexOf(' ', classIndex) + 1;
                         var classNameLength =
                             src.IndexOf(' ', classNameIndex) - classNameIndex;

                         var typeName =
                             src.Substring(classNameIndex, classNameLength);
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

                         return new BinarySchemaStructureParser()
                             .ParseStructure(
                                 namedTypeSymbol);
                       })
                       .ToArray();

      var structureByNamedTypeSymbol =
          new Dictionary<INamedTypeSymbol, IBinarySchemaStructure>();
      foreach (var structure in structures) {
        structureByNamedTypeSymbol[structure.TypeSymbol] = structure;
      }

      var sizeOfMemberInBytesDependencyFixer =
          new WSizeOfMemberInBytesDependencyFixer();
      foreach (var structure in structures) {
        foreach (var member in structure.Members) {
          if (member.MemberType is IPrimitiveMemberType primitiveMemberType) {
            if (primitiveMemberType.AccessChainToSizeOf != null) {
              sizeOfMemberInBytesDependencyFixer.AddDependenciesForStructure(
                  structureByNamedTypeSymbol,
                  primitiveMemberType.AccessChainToSizeOf);
            }

            if (primitiveMemberType.AccessChainToPointer != null) {
              sizeOfMemberInBytesDependencyFixer.AddDependenciesForStructure(
                  structureByNamedTypeSymbol,
                  primitiveMemberType.AccessChainToPointer);
            }
          }
        }
      }

      return structures;
    }

    public static void AssertDiagnostics(
        IList<Diagnostic> actualDiagnostics,
        params DiagnosticDescriptor[] expectedDiagnostics) {
      var message = "";

      if (actualDiagnostics.Count != expectedDiagnostics.Length) {
        message +=
            $"Expected {expectedDiagnostics.Length} diagnostics but got {actualDiagnostics.Count}.\n";
      }

      bool[] actualMatches = new bool[actualDiagnostics.Count];
      bool[] expectedMatches = new bool[expectedDiagnostics.Length];

      for (var a = 0; a < actualDiagnostics.Count; ++a) {
        var actualDiagnostic = actualDiagnostics[a];

        for (var e = 0; e < expectedDiagnostics.Length; ++e) {
          if (expectedMatches[e]) {
            continue;
          }

          var expectedDiagnostic = expectedDiagnostics[e];
          if (actualDiagnostic.Descriptor.Equals(expectedDiagnostic)) {
            actualMatches[a] = true;
            expectedMatches[a] = true;
            break;
          }
        }
      }

      var allActualMatched = actualMatches.All(value => value);
      var allExpectedMatched = expectedMatches.All(value => value);

      if (allActualMatched && allExpectedMatched) {
        return;
      }

      if (!allActualMatched) {
        message += "\n";
        message += "Unexpected actual diagnostics:\n";

        var i = 0;
        foreach (var (hadMatch, actualDiagnostic) in Enumerable.Zip(
                     actualMatches,
                     actualDiagnostics)) {
          if (hadMatch) {
            continue;
          }

          message += $" {i++}) {actualDiagnostic.GetMessage()}\n";
        }
      }

      if (!allExpectedMatched) {
        message += "\n";
        message += "Unmatched expected diagnostics:\n";

        var i = 0;
        foreach (var (hadMatch, expectedDiagnostic) in Enumerable.Zip(
                     expectedMatches,
                     expectedDiagnostics)) {
          if (hadMatch) {
            continue;
          }

          message += $" {i++}) {expectedDiagnostic.MessageFormat}\n";
        }
      }

      if (message.Length != 0) {
        Assert.Fail(message);
      }
    }

    public static void AssertGenerated(string src,
                                       string expectedReader,
                                       string expectedWriter) {
      var structure = BinarySchemaTestUtil.ParseFirst(src);
      Assert.IsEmpty(structure.Diagnostics);

      var actualReader = new BinarySchemaReaderGenerator().Generate(structure);
      var actualWriter = new BinarySchemaWriterGenerator().Generate(structure);

      Assert.AreEqual(expectedReader, actualReader.ReplaceLineEndings());
      Assert.AreEqual(expectedWriter, actualWriter.ReplaceLineEndings());
    }

    public static void AssertGeneratedForAll(
        string src,
        params (string, string)[] expectedReadersAndWriters) {
      var structures = BinarySchemaTestUtil.ParseAll(src).ToArray();
      Assert.AreEqual(expectedReadersAndWriters.Length, structures.Length);
      for (var i = 0; i < structures.Length; ++i) {
        var (expectedReader, expectedWriter) = expectedReadersAndWriters[i];
        var structure = structures[i];

        Assert.IsEmpty(structure.Diagnostics);

        var actualReader =
            new BinarySchemaReaderGenerator().Generate(structure);
        var actualWriter =
            new BinarySchemaWriterGenerator().Generate(structure);

        Assert.AreEqual(expectedReader, actualReader.ReplaceLineEndings());
        Assert.AreEqual(expectedWriter, actualWriter.ReplaceLineEndings());
      }
    }
  }
}