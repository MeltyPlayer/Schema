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

[AttributeUsage(AttributeTargets.Class |
                AttributeTargets.Interface |
                AttributeTargets.Struct)]
public class GenerateReadOnlyAttribute : Attribute;

[AttributeUsage(AttributeTargets.Method)]
public class ConstAttribute : Attribute;

[AttributeUsage(AttributeTargets.GenericParameter |
                AttributeTargets.Parameter |
                AttributeTargets.Property |
                AttributeTargets.Method)]
public class KeepMutableTypeAttribute : Attribute;

[Generator(LanguageNames.CSharp)]
public class ReadOnlyTypeGenerator
    : BNamedTypesWithAttributeGenerator<GenerateReadOnlyAttribute> {
  private static readonly TypeInfoParser parser_ = new();

  internal override bool FilterNamedTypesBeforeGenerating(
      TypeDeclarationSyntax syntax,
      INamedTypeSymbol symbol) => true;

  internal override IEnumerable<(string fileName, string source)>
      GenerateSourcesForNamedType(INamedTypeSymbol symbol,
                                  SemanticModel semanticModel,
                                  TypeDeclarationSyntax syntax) {
      yield return ($"{symbol.GetUniqueNameForGenerator()}_readOnly.g",
                    this.GenerateSourceForNamedType(
                        symbol,
                        semanticModel,
                        syntax));
    }

  public string GenerateSourceForNamedType(INamedTypeSymbol typeSymbol,
                                           SemanticModel semanticModel,
                                           TypeDeclarationSyntax syntax) {
      var sb = new StringBuilder();
      using var sw = new SourceWriter(new StringWriter(sb));

      sw.WriteNamespaceAndParentTypeBlocks(
          typeSymbol,
          () => {
            var interfaceName = typeSymbol.GetConstInterfaceName();

            var constMembers
                = parser_
                  .ParseMembers(typeSymbol)
                  .Where(parsedMember => {
                    var (parseStatus, memberSymbol, _, _) = parsedMember;
                    if (parseStatus ==
                        TypeInfoParser.ParseStatus
                                      .NOT_A_FIELD_OR_PROPERTY_OR_METHOD) {
                      return false;
                    }

                    if (memberSymbol.DeclaredAccessibility is not (
                        Accessibility.Public
                        or Accessibility.Internal)) {
                      return false;
                    }

                    if (memberSymbol is IFieldSymbol) {
                      return false;
                    }

                    if (memberSymbol is IPropertySymbol) {
                      return false;
                    }

                    if (memberSymbol is IMethodSymbol &&
                        !memberSymbol.Name.StartsWith("get_") &&
                        !memberSymbol.HasAttribute<ConstAttribute>()) {
                      return false;
                    }

                    return true;
                  })
                  .Select(parsedMember => (IMethodSymbol) parsedMember.Item2)
                  .ToArray();

            // Class
            {
              var blockPrefix =
                  typeSymbol.GetQualifiersAndNameAndGenericParametersFor() +
                  " : " +
                  typeSymbol.GetNameAndGenericParametersFor(interfaceName);

              if (constMembers.Length == 0) {
                sw.Write(blockPrefix).WriteLine(";");
              } else {
                sw.EnterBlock(blockPrefix);
                WriteMembers_(sw,
                              typeSymbol,
                              constMembers,
                              semanticModel,
                              syntax,
                              interfaceName);
                sw.ExitBlock();
              }
            }
            sw.WriteLine("");

            // Interface
            {
              sw.WriteLine("#nullable enable");
              sw.Write(
                  SymbolTypeUtil.AccessibilityToModifier(
                      typeSymbol.DeclaredAccessibility));
              sw.Write(" interface ");

              var blockPrefix = interfaceName;
              blockPrefix
                  += typeSymbol
                      .GetGenericParametersWithVarianceForReadOnlyVersion(
                          constMembers);
              var parentConstNames =
                  GetDirectBaseTypeAndInterfaces_(typeSymbol)
                      .Where(i => i.HasAttribute<GenerateReadOnlyAttribute>() ||
                                  IsTypeAlreadyConst_(i))
                      .Select(i => typeSymbol
                                  .GetQualifiedNameAndGenericsOrReadOnlyFromCurrentSymbol(
                                      i,
                                      semanticModel,
                                      syntax))
                      .ToArray();
              if (parentConstNames.Length > 0) {
                blockPrefix += " : " + string.Join(", ", parentConstNames);
              }

              blockPrefix += typeSymbol.GetTypeConstraintsOrReadonly(
                  typeSymbol.TypeParameters,
                  semanticModel,
                  syntax);

              if (constMembers.Length == 0) {
                sw.Write(blockPrefix).WriteLine(";");
              } else {
                sw.EnterBlock(blockPrefix);
                WriteMembers_(sw,
                              typeSymbol,
                              constMembers,
                              semanticModel,
                              syntax);
                sw.ExitBlock();
              }
            }
          });

      return sb.ToString();
    }

  private static bool IsTypeAlreadyConst_(INamedTypeSymbol typeSymbol) {
      if (typeSymbol.IsType(typeof(IEnumerable<>))) {
        return true;
      }

      foreach (var parsedMember in parser_.ParseMembers(
                   typeSymbol)) {
        var (parseStatus, memberSymbol, _, _)
            = parsedMember;
        if (parseStatus ==
            TypeInfoParser.ParseStatus.NOT_A_FIELD_OR_PROPERTY_OR_METHOD) {
          continue;
        }

        if (memberSymbol.DeclaredAccessibility is not (Accessibility.Public
            or Accessibility.Internal)) {
          continue;
        }

        if (memberSymbol is IFieldSymbol) {
          continue;
        }

        if (memberSymbol is IPropertySymbol {IsReadOnly: true}) {
          continue;
        }

        if (memberSymbol is IMethodSymbol &&
            (memberSymbol.Name.StartsWith("get_") ||
             memberSymbol.HasAttribute<ConstAttribute>())) {
          continue;
        }

        return false;
      }

      return GetDirectBaseTypeAndInterfaces_(typeSymbol)
          .All(IsTypeAlreadyConst_);
    }

  private static void WriteMembers_(
      ISourceWriter sw,
      INamedTypeSymbol typeSymbol,
      IReadOnlyList<IMethodSymbol> constMembers,
      SemanticModel semanticModel,
      TypeDeclarationSyntax syntax,
      string? interfaceName = null) {
      foreach (var memberSymbol in constMembers) {
        var memberTypeSymbol = memberSymbol.ReturnType;

        if (interfaceName == null) {
          sw.Write(SymbolTypeUtil.AccessibilityToModifier(
                       typeSymbol.DeclaredAccessibility))
            .Write(" ");
        }

        IPropertySymbol? associatedPropertySymbol
            = memberSymbol.AssociatedSymbol as IPropertySymbol;
        sw.Write(
              typeSymbol
                  .GetQualifiedNameAndGenericsOrReadOnlyFromCurrentSymbol(
                      memberTypeSymbol,
                      semanticModel,
                      syntax,
                      (ISymbol?) associatedPropertySymbol ?? memberSymbol))
          .Write(" ");

        if (interfaceName != null) {
          sw.Write(interfaceName)
            .Write(typeSymbol.GetGenericParameters())
            .Write(".");
        }

        // Property
        if (memberSymbol.IsPropertyGetter(out var propertyAccessName)) {
          var isIndexer
              = memberSymbol.IsIndexer(out var indexerParameterSymbols);

          if (!isIndexer) {
            propertyAccessName = propertyAccessName.EscapeKeyword();
            sw.Write(memberSymbol.Name.Substring(4).EscapeKeyword());
          } else {
            propertyAccessName = "this";
            sw.Write("this[");
            for (var i = 0; i < indexerParameterSymbols.Length; ++i) {
              if (i > 0) {
                sw.Write(", ");
              }

              var parameterSymbol = indexerParameterSymbols[i];
              sw.Write(
                    typeSymbol.GetQualifiedNameAndGenericsFromCurrentSymbol(
                        parameterSymbol.Type,
                        semanticModel,
                        syntax,
                        parameterSymbol))
                .Write(" ")
                .Write(parameterSymbol.Name.EscapeKeyword());
            }

            sw.Write("]");
          }

          if (interfaceName == null) {
            sw.WriteLine(" { get; }");
          } else {
            sw.Write(" => ")
              .Write(typeSymbol.GetCStyleCastToReadOnlyIfNeeded(
                         associatedPropertySymbol,
                         memberSymbol.ReturnType,
                         semanticModel,
                         syntax))
              .Write(propertyAccessName);

            if (isIndexer) {
              sw.Write("[");
              for (var i = 0; i < memberSymbol.Parameters.Length; ++i) {
                if (i > 0) {
                  sw.Write(", ");
                }

                var parameterSymbol = memberSymbol.Parameters[i];
                sw.Write(parameterSymbol.Name.EscapeKeyword());
              }

              sw.Write("]");
            }

            sw.WriteLine(";");
          }
        }
        // Method
        else {
          var accessName = memberSymbol.Name.EscapeKeyword();
          sw.Write(accessName);
          sw.Write(memberSymbol.TypeParameters
                               .GetGenericParameters());
          sw.Write("(");

          for (var i = 0; i < memberSymbol.Parameters.Length; ++i) {
            if (i > 0) {
              sw.Write(", ");
            }

            var parameterSymbol = memberSymbol.Parameters[i];
            if (parameterSymbol.IsParams) {
              sw.Write("params ");
            }

            var refKindString = parameterSymbol.RefKind.GetRefKindString();
            if (refKindString.Length > 0) {
              sw.Write(refKindString).Write(" ");
            }

            sw.Write(
                  typeSymbol.GetQualifiedNameAndGenericsFromCurrentSymbol(
                      parameterSymbol.Type,
                      semanticModel,
                      syntax,
                      parameterSymbol))
              .Write(" ")
              .Write(parameterSymbol.Name.EscapeKeyword());


            if (interfaceName == null &&
                parameterSymbol.HasExplicitDefaultValue) {
              var defaultValueType = parameterSymbol.Type.UnwrapNullable();

              sw.Write(" = ");

              var explicitDefaultValue = parameterSymbol.ExplicitDefaultValue;
              if (defaultValueType.IsEnum(out _) &&
                  explicitDefaultValue != null) {
                sw.Write(
                    $"({typeSymbol.GetQualifiedNameFromCurrentSymbol(defaultValueType)}) {explicitDefaultValue}");
              } else {
                switch (explicitDefaultValue) {
                  case null:
                    sw.Write("null");
                    break;
                  case char:
                    sw.Write($"'{explicitDefaultValue}'");
                    break;
                  case string:
                    sw.Write($"\"{explicitDefaultValue}\"");
                    break;
                  case bool boolValue:
                    sw.Write(boolValue ? "true" : "false");
                    break;
                  default:
                    sw.Write(explicitDefaultValue.ToString());
                    break;
                }
              }
            }
          }

          sw.Write(")");

          if (interfaceName == null) {
            sw.Write(typeSymbol.GetTypeConstraintsOrReadonly(
                         memberSymbol.TypeParameters,
                         semanticModel,
                         syntax));
          }

          if (interfaceName == null) {
            sw.WriteLine(";");
          } else {
            sw.Write(" => ")
              .Write(typeSymbol.GetCStyleCastToReadOnlyIfNeeded(
                         memberSymbol,
                         memberSymbol.ReturnType,
                         semanticModel,
                         syntax))
              .Write(accessName)
              .Write(memberSymbol.TypeParameters.GetGenericParameters())
              .Write("(");
            for (var i = 0; i < memberSymbol.Parameters.Length; ++i) {
              if (i > 0) {
                sw.Write(", ");
              }

              var parameterSymbol = memberSymbol.Parameters[i];

              var refKindString = parameterSymbol.RefKind.GetRefKindString();
              if (refKindString.Length > 0) {
                sw.Write(refKindString).Write(" ");
              }

              sw.Write(parameterSymbol.Name.EscapeKeyword());
            }

            sw.WriteLine(");");
          }
        }
      }
    }

  private static IEnumerable<INamedTypeSymbol>
      GetDirectBaseTypeAndInterfaces_(
          INamedTypeSymbol symbol) {
      var baseType = symbol.BaseType;
      if (baseType != null &&
          !baseType.IsType<object>() &&
          !baseType.IsType<ValueType>()) {
        yield return baseType;
      }

      var parentInterfaces
          = symbol.Interfaces
                  .Where(i => !(i.IsType(typeof(IEquatable<>)) &&
                                i.TypeArguments[0]
                                 .GetFullyQualifiedNamespace() ==
                                symbol.GetFullyQualifiedNamespace() &&
                                i.TypeArguments[0].Name == symbol.Name));

      foreach (var iface in parentInterfaces) {
        yield return iface;
      }
    }
}

internal static class ReadOnlyTypeGeneratorUtil {
  public const string PREFIX = "IReadOnly";

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
      TypeDeclarationSyntax syntax) {
      var sb = new StringBuilder();

      foreach (var typeParameter in typeParameters) {
        var typeConstraintNames
            = sourceSymbol
              .GetTypeConstraintNames_(typeParameter, semanticModel, syntax)
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
      TypeDeclarationSyntax sourceDeclarationSyntax) {
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
                                    sourceDeclarationSyntax));

        yield return typeParameter.ConstraintNullableAnnotations[i] ==
                     NullableAnnotation.Annotated
            ? $"{qualifiedName}?"
            : qualifiedName;
      }
    }

  private static string ConvertName_(ITypeSymbol typeSymbol,
                                     ISymbol? symbolForAttributeChecks) {
      var defaultName = typeSymbol.Name.EscapeKeyword();
      if ((symbolForAttributeChecks ?? typeSymbol)
          .HasAttribute<KeepMutableTypeAttribute>()) {
        return defaultName;
      }

      if (typeSymbol.HasBuiltInReadOnlyType_(out var builtInReadOnlyName,
                                             out _)) {
        return builtInReadOnlyName;
      }

      return typeSymbol.HasAttribute<GenerateReadOnlyAttribute>()
          ? typeSymbol.GetConstInterfaceName()
          : defaultName;
    }

  private static bool HasBuiltInReadOnlyType_(
      this ITypeSymbol symbol,
      out string readOnlyName,
      out bool canImplicitlyConvert) {
      if (symbol.IsType(typeof(Span<>))) {
        readOnlyName = typeof(ReadOnlySpan<>).GetCorrectName();
        canImplicitlyConvert = true;
        return true;
      }

      if (symbol.IsType(typeof(IList<>))) {
        readOnlyName = typeof(IReadOnlyList<>).GetCorrectName();
        canImplicitlyConvert = false;
        return true;
      }

      if (symbol.IsType(typeof(ICollection<>))) {
        readOnlyName = typeof(IReadOnlyCollection<>).GetCorrectName();
        canImplicitlyConvert = false;
        return true;
      }

      readOnlyName = default;
      canImplicitlyConvert = false;
      return false;
    }

  public static string GetConstInterfaceName(
      this ITypeSymbol typeSymbol) {
      var baseName = typeSymbol.Name;
      if (baseName.Length >= 2) {
        if (baseName[1] is < 'a' or > 'z') {
          var firstChar = baseName[0];
          if ((firstChar == 'I' && typeSymbol.IsInterface()) ||
              (firstChar == 'B' && typeSymbol.IsAbstractClass())) {
            baseName = baseName.Substring(1);
          }
        }
      }

      return $"{PREFIX}{baseName}";
    }

  public static IEnumerable<INamedTypeSymbol> LookupTypesWithNameAndArity(
      SemanticModel semanticModel,
      TypeDeclarationSyntax syntax,
      string searchString,
      int arity)
    => semanticModel
       .LookupNamespacesAndTypes(syntax.SpanStart, null, searchString)
       .Where(symbol => symbol.HasAttribute<GenerateReadOnlyAttribute>())
       .Where(symbol => symbol is INamedTypeSymbol)
       .Select(symbol => symbol as INamedTypeSymbol)
       .Where(symbol => symbol.Arity == arity);

  public static IEnumerable<string> GetNamespaceOfType(
      this ITypeSymbol typeSymbol,
      SemanticModel semanticModel,
      TypeDeclarationSyntax syntax) {
      if (!typeSymbol.Exists()) {
        var typeName = typeSymbol.Name;
        if (typeName.StartsWith(
                ReadOnlyTypeGeneratorUtil.PREFIX)) {
          var nameWithoutPrefix
              = typeName.Substring(
                  ReadOnlyTypeGeneratorUtil.PREFIX.Length);
          var arity = typeSymbol.GetArity();

          var typesWithName
              = LookupTypesWithNameAndArity(semanticModel,
                                            syntax,
                                            nameWithoutPrefix,
                                            arity)
                  .ToArray();
          if (typesWithName.Length == 0) {
            typesWithName = LookupTypesWithNameAndArity(semanticModel,
                  syntax,
                  $"B{nameWithoutPrefix}",
                  arity)
                .ToArray();
          }

          if (typesWithName.Length == 0) {
            typesWithName = LookupTypesWithNameAndArity(semanticModel,
                  syntax,
                  $"I{nameWithoutPrefix}",
                  arity)
                .ToArray();
          }

          if (typesWithName.Length == 1) {
            var typeWithName = typesWithName[0];
            return typeWithName.GetContainingNamespaces();
          }
        }
      }

      return typeSymbol.GetContainingNamespaces();
    }

  public static string GetGenericParametersWithVarianceForReadOnlyVersion(
      this INamedTypeSymbol symbol,
      IReadOnlyList<IMethodSymbol> constMembers) {
      var typeParameters = symbol.TypeParameters;
      if (typeParameters.Length == 0) {
        return "";
      }

      var allParentTypes
          = symbol.GetBaseTypes().Concat(symbol.AllInterfaces).ToArray();

      var set = new TypeParameterSymbolVarianceSet(
          typeParameters,
          allParentTypes,
          constMembers);

      var sb = new StringBuilder();
      sb.Append("<");
      for (var i = 0; i < typeParameters.Length; ++i) {
        if (i > 0) {
          sb.Append(", ");
        }

        var typeParameter = typeParameters[i];

        var variance = typeParameter.Variance;
        if (variance == VarianceKind.None) {
          variance = set.AllowedVariance(typeParameter);
        }

        sb.Append(variance switch {
              VarianceKind.In   => "in ",
              VarianceKind.Out  => "out ",
              VarianceKind.None => "",
          })
          .Append(typeParameter.Name.EscapeKeyword());
      }

      sb.Append(">");
      return sb.ToString();
    }

  public static string GetCStyleCastToReadOnlyIfNeeded(
      this ITypeSymbol sourceSymbol,
      ISymbol? symbolForAttributeChecks,
      ITypeSymbol symbol,
      SemanticModel semanticModel,
      TypeDeclarationSyntax syntax) {
      // TODO: Only allow casts if generics are covariant, otherwise report
      // diagnostic error
      var sb = new StringBuilder();
      if (symbol.IsCastNeeded(symbolForAttributeChecks)) {
        sb.Append("(")
          .Append(
              sourceSymbol
                  .GetQualifiedNameAndGenericsOrReadOnlyFromCurrentSymbol(
                      symbol,
                      semanticModel,
                      syntax))
          .Append(")(object) ");
      }

      return sb.ToString();
    }

  public static bool IsCastNeeded(this ITypeSymbol symbol,
                                  ISymbol? symbolForAttributeChecks) {
      if ((symbolForAttributeChecks ?? symbol)
          .HasAttribute<KeepMutableTypeAttribute>()) {
        return false;
      }

      if (symbol.IsGeneric(out _, out var typeArguments) &&
          typeArguments.Any(typeArgument => typeArgument
                                .HasAttribute<GenerateReadOnlyAttribute>())) {
        return true;
      }

      return symbol.HasBuiltInReadOnlyType_(out _,
                                            out var canImplicitlyConvert) &&
             !canImplicitlyConvert;
    }
}