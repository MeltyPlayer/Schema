using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace schema.util {
  public static class SourceUtil {
    public static IEnumerable<ClassDeclarationSyntax>
        GetClassesWithAttribute<TAttribute>(
            GeneratorExecutionContext context,
            TAttribute attributeName) where TAttribute : Attribute
      => context.Compilation.SyntaxTrees
                .SelectMany(
                    syntaxTree => syntaxTree.GetRoot().DescendantNodes())
                .Where(x => x is ClassDeclarationSyntax)
                .Cast<ClassDeclarationSyntax>()
                .Where(SourceUtil.HasAttribute<TAttribute>);

    public static bool HasAttribute<TAttribute>(
        MemberDeclarationSyntax source) where TAttribute : Attribute {
      var attributeName =
          typeof(TAttribute).Name.Replace("Attribute", string.Empty);
      return source.AttributeLists.Any(
          x => x.Attributes.Any(
              c => c.Name.GetText().ToString() == attributeName));
    }

    public static bool IsPartial(ClassDeclarationSyntax source)
      => source.IsKind(SyntaxKind.PartialKeyword);
  }
}