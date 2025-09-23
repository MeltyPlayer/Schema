using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using schema.binary.parser;
using schema.util.generators;
using schema.util.symbols;
using schema.util.text;
using schema.util.types;


namespace schema.readOnly;

internal class GeneratorUtilContext(
    IReadOnlyDictionary<(string name, int arity), IEnumerable<string>?>
        knownNamespaces) {
  public IReadOnlyDictionary<(string name, int arity), IEnumerable<string>?>
      KnownNamespaces { get; } = knownNamespaces;
}

internal static class ReadOnlyTypeGeneratorUtil {
  public static string
      GetQualifiedNameAndGenericsOrReadOnlyFromCurrentSymbol(
          this ITypeSymbol sourceSymbol,
          ITypeSymbol referencedSymbol,
          SemanticModel semanticModel,
          TypeDeclarationSyntax sourceDeclarationSyntax,
          ISymbol? memberSymbol = null)
    => sourceSymbol.GetQualifiedNameFromCurrentSymbol(
        referencedSymbol,
        memberSymbol,
        ConvertName_,
        r => GetNamespaceOfType(r, semanticModel, sourceDeclarationSyntax));

  public static string GetQualifiedNameAndGenericsFromCurrentSymbol(
      this ITypeSymbol sourceSymbol,
      ITypeSymbol referencedSymbol,
      SemanticModel semanticModel,
      TypeDeclarationSyntax sourceDeclarationSyntax,
      ISymbol? memberSymbol = null)
    => sourceSymbol.GetQualifiedNameFromCurrentSymbol(
        referencedSymbol,
        memberSymbol,
        null,
        r => GetNamespaceOfType(r, semanticModel, sourceDeclarationSyntax));

  public static string GetTypeConstraintsOrReadonly(
      this ITypeSymbol sourceSymbol,
      IReadOnlyList<ITypeParameterSymbol> typeParameters,
      SemanticModel semanticModel,
      TypeDeclarationSyntax syntax,
      GeneratorUtilContext? context = null) {
    var sb = new StringBuilder();

    foreach (var typeParameter in typeParameters) {
      var typeConstraintNames
          = sourceSymbol
            .GetTypeConstraintNames_(typeParameter,
                                     semanticModel,
                                     syntax,
                                     context)
            .ToArray();
      if (typeConstraintNames.Length == 0) {
        continue;
      }

      sb.Append(" where ")
        .Append(typeParameter.Name.EscapeKeyword())
        .Append(" : ");

      for (var i = 0; i < typeConstraintNames.Length; ++i) {
        if (i > 0) {
          sb.Append(", ");
        }

        sb.Append(typeConstraintNames[i]);
      }
    }

    return sb.ToString();
  }

  private static IEnumerable<string> GetTypeConstraintNames_(
      this ITypeSymbol sourceSymbol,
      ITypeParameterSymbol typeParameter,
      SemanticModel semanticModel,
      TypeDeclarationSyntax sourceDeclarationSyntax,
      GeneratorUtilContext? context = null) {
    if (typeParameter.HasNotNullConstraint) {
      yield return "notnull";
    }

    if (typeParameter.HasConstructorConstraint) {
      yield return "new()";
    }

    if (typeParameter.HasUnmanagedTypeConstraint) {
      yield return "unmanaged";
    }

    if (typeParameter.HasReferenceTypeConstraint) {
      yield return typeParameter
                       .ReferenceTypeConstraintNullableAnnotation ==
                   NullableAnnotation.Annotated
          ? "class?"
          : "class";
    }

    if (typeParameter is {
            HasValueTypeConstraint: true, HasUnmanagedTypeConstraint: false
        }) {
      yield return "struct";
    }

    for (var i = 0; i < typeParameter.ConstraintTypes.Length; ++i) {
      var constraintType = typeParameter.ConstraintTypes[i];
      var qualifiedName = sourceSymbol.GetQualifiedNameFromCurrentSymbol(
          constraintType,
          typeParameter,
          ConvertName_,
          r => GetNamespaceOfType(r,
                                  semanticModel,
                                  sourceDeclarationSyntax,
                                  context));

      yield return typeParameter.ConstraintNullableAnnotations[i] ==
                   NullableAnnotation.Annotated
          ? $"{qualifiedName}?"
          : qualifiedName;
    }
  }

  private static string ConvertName_(ITypeSymbol typeSymbol,
                                     ISymbol? symbolForAttributeChecks)
    => typeSymbol.Name.EscapeKeyword();

  public static IEnumerable<string>? GetNamespaceOfType(
      this ITypeSymbol typeSymbol,
      SemanticModel semanticModel,
      TypeDeclarationSyntax syntax,
      GeneratorUtilContext? context = null) {
    if (!typeSymbol.Exists()) {
      var typeName = typeSymbol.Name;
      var arity = typeSymbol.GetArity();

      if (context?.KnownNamespaces.TryGetValue((typeName, arity),
                                               out var knownNamespace) ??
          false) {
        return knownNamespace;
      }
    }

    return typeSymbol.GetContainingNamespaces();
  }
}