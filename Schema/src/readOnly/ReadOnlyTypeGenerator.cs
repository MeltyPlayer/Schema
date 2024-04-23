using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using schema.binary.parser;
using schema.util.asserts;
using schema.util.generators;
using schema.util.symbols;
using schema.util.text;
using schema.util.types;

namespace schema.readOnly {
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
      var typeV2 = TypeV2.FromSymbol(symbol);
      yield return ($"{typeV2.FullyQualifiedName}_readOnly.g",
                    this.GenerateSourceForNamedType(
                        symbol,
                        semanticModel,
                        syntax));
    }

    public string GenerateSourceForNamedType(INamedTypeSymbol typeSymbol,
                                             SemanticModel semanticModel,
                                             TypeDeclarationSyntax syntax) {
      var typeNamespace = typeSymbol.GetFullyQualifiedNamespace();

      var declaringTypes = typeSymbol.GetDeclaringTypesDownward();

      var sb = new StringBuilder();
      using var cbsb = new CurlyBracketTextWriter(new StringWriter(sb));

      // TODO: Handle fancier cases here
      if (typeNamespace != null) {
        cbsb.EnterBlock($"namespace {typeNamespace}");
      }

      foreach (var declaringType in declaringTypes) {
        cbsb.EnterBlock(declaringType
                            .GetQualifiersAndNameAndGenericParametersFor());
      }

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
                  Accessibility.Public or Accessibility.Internal)) {
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
          cbsb.Write(blockPrefix).WriteLine(";");
        } else {
          cbsb.EnterBlock(blockPrefix);
          WriteMembers_(cbsb,
                        typeSymbol,
                        constMembers,
                        semanticModel,
                        syntax,
                        interfaceName);
          cbsb.ExitBlock();
        }
      }
      cbsb.WriteLine("");

      // Interface
      {
        cbsb.Write(
            SymbolTypeUtil.AccessibilityToModifier(
                typeSymbol.DeclaredAccessibility));
        cbsb.Write(" interface ");

        var blockPrefix = interfaceName;
        blockPrefix
            += typeSymbol.GetGenericParametersWithVarianceForReadOnlyVersion(constMembers);
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
          cbsb.Write(blockPrefix).WriteLine(";");
        } else {
          cbsb.EnterBlock(blockPrefix);
          WriteMembers_(cbsb, typeSymbol, constMembers, semanticModel, syntax);
          cbsb.ExitBlock();
        }

        // parent types
        foreach (var _ in declaringTypes) {
          cbsb.ExitBlock();
        }

        // namespace
        if (typeNamespace != null) {
          cbsb.ExitBlock();
        }

        return sb.ToString();
      }
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

        if (memberSymbol is IPropertySymbol { IsReadOnly: true }) {
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
        ICurlyBracketTextWriter cbsb,
        INamedTypeSymbol typeSymbol,
        IReadOnlyList<IMethodSymbol> constMembers,
        SemanticModel semanticModel,
        TypeDeclarationSyntax syntax,
        string? interfaceName = null) {
      foreach (var memberSymbol in constMembers) {
        var memberTypeSymbol = memberSymbol.ReturnType;

        if (interfaceName == null) {
          cbsb.Write(
              SymbolTypeUtil.AccessibilityToModifier(
                  typeSymbol.DeclaredAccessibility));
          cbsb.Write(" ");
        }

        IPropertySymbol? associatedPropertySymbol
            = memberSymbol.AssociatedSymbol as IPropertySymbol;
        cbsb.Write(
            typeSymbol.GetQualifiedNameAndGenericsOrReadOnlyFromCurrentSymbol(
                memberTypeSymbol,
                semanticModel,
                syntax,
                associatedPropertySymbol));
        cbsb.Write(" ");

        if (interfaceName != null) {
          cbsb.Write(interfaceName);
          cbsb.Write(typeSymbol.GetGenericParameters());
          cbsb.Write(".");
        }

        // Property
        if (memberSymbol.IsPropertyGetter(out var propertyAccessName)) {
          var isIndexer
              = memberSymbol.IsIndexer(out var indexerParameterSymbols);

          if (!isIndexer) {
            propertyAccessName = propertyAccessName.EscapeKeyword();
            cbsb.Write(memberSymbol.Name.Substring(4).EscapeKeyword());
          } else {
            propertyAccessName = "this";
            cbsb.Write("this[");
            for (var i = 0; i < indexerParameterSymbols.Length; ++i) {
              if (i > 0) {
                cbsb.Write(", ");
              }

              var parameterSymbol = indexerParameterSymbols[i];
              cbsb.Write(
                  typeSymbol
                      .GetQualifiedNameAndGenericsOrReadOnlyFromCurrentSymbol(
                          parameterSymbol.Type,
                          semanticModel,
                          syntax,
                          parameterSymbol));
              cbsb.Write(" ");
              cbsb.Write(parameterSymbol.Name.EscapeKeyword());
            }

            cbsb.Write("]");
          }

          if (interfaceName == null) {
            cbsb.WriteLine(" { get; }");
          } else {
            cbsb.Write(" => ");

            cbsb.Write(propertyAccessName);

            if (isIndexer) {
              cbsb.Write("[");
              for (var i = 0; i < memberSymbol.Parameters.Length; ++i) {
                if (i > 0) {
                  cbsb.Write(", ");
                }

                var parameterSymbol = memberSymbol.Parameters[i];
                cbsb.Write(parameterSymbol.Name.EscapeKeyword());
              }

              cbsb.Write("]");
            }

            cbsb.WriteLine(";");
          }
        }
        // Method
        else {
          var accessName = memberSymbol.Name.EscapeKeyword();
          cbsb.Write(accessName);
          cbsb.Write(memberSymbol.TypeParameters
                                 .GetGenericParameters());
          cbsb.Write("(");

          for (var i = 0; i < memberSymbol.Parameters.Length; ++i) {
            if (i > 0) {
              cbsb.Write(", ");
            }

            var parameterSymbol = memberSymbol.Parameters[i];
            if (parameterSymbol.IsParams) {
              cbsb.Write("params ");
            }

            var refKindString = parameterSymbol.RefKind.GetRefKindString();
            if (refKindString.Length > 0) {
              cbsb.Write(refKindString).Write(" ");
            }

            cbsb.Write(
                    typeSymbol
                        .GetQualifiedNameAndGenericsOrReadOnlyFromCurrentSymbol(
                            parameterSymbol.Type,
                            semanticModel,
                            syntax,
                            parameterSymbol))
                .Write(" ")
                .Write(parameterSymbol.Name.EscapeKeyword());

            if (interfaceName == null &&
                parameterSymbol.HasExplicitDefaultValue) {
              cbsb.Write(" = ");

              var explicitDefaultValue = parameterSymbol.ExplicitDefaultValue;
              switch (explicitDefaultValue) {
                case null:
                  cbsb.Write("null");
                  break;
                case char:
                  cbsb.Write($"'{explicitDefaultValue}'");
                  break;
                case string:
                  cbsb.Write($"\"{explicitDefaultValue}\"");
                  break;
                default:
                  cbsb.Write(explicitDefaultValue.ToString());
                  break;
              }
            }
          }

          cbsb.Write(")");

          if (interfaceName == null) {
            cbsb.Write(typeSymbol.GetTypeConstraintsOrReadonly(
                           memberSymbol.TypeParameters,
                           semanticModel,
                           syntax));
          }

          if (interfaceName == null) {
            cbsb.WriteLine(";");
          } else {
            cbsb.Write(" => ")
                .Write(accessName)
                .Write(memberSymbol.TypeParameters.GetGenericParameters())
                .Write("(");
            for (var i = 0; i < memberSymbol.Parameters.Length; ++i) {
              if (i > 0) {
                cbsb.Write(", ");
              }

              var parameterSymbol = memberSymbol.Parameters[i];

              var refKindString = parameterSymbol.RefKind.GetRefKindString();
              if (refKindString.Length > 0) {
                cbsb.Write(refKindString).Write(" ");
              }

              cbsb.Write(parameterSymbol.Name.EscapeKeyword());
            }

            cbsb.Write(")");
            cbsb.WriteLine(";");
          }
        }
      }
    }

    private static IEnumerable<INamedTypeSymbol>
        GetDirectBaseTypeAndInterfaces_(
            INamedTypeSymbol symbol) {
      var baseType = symbol.BaseType;
      if (baseType != null &&
          !SymbolComparisonUtil.IsType((ISymbol) baseType, typeof(object)) &&
          !SymbolComparisonUtil.IsType((ISymbol) baseType, typeof(ValueType))) {
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

    public static string GetQualifiedNameAndGenericsOrReadOnlyFromCurrentSymbol(
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

        sb.Append(" where ");
        sb.Append(typeParameter.Name.EscapeKeyword());
        sb.Append(" : ");

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
        yield return typeParameter.ReferenceTypeConstraintNullableAnnotation ==
                     NullableAnnotation.Annotated
            ? "class?"
            : "class";
      }

      if (typeParameter is { HasValueTypeConstraint: true, HasUnmanagedTypeConstraint: false }) {
        yield return "struct";
      }

      for (var i = 0; i < typeParameter.ConstraintTypes.Length; ++i) {
        var constraintType = typeParameter.ConstraintTypes[i];
        var qualifiedName = sourceSymbol.GetQualifiedNameFromCurrentSymbol(
            constraintType,
            typeParameter,
            ConvertName_,
            r => GetNamespaceOfType(r, semanticModel, sourceDeclarationSyntax));

        yield return typeParameter.ConstraintNullableAnnotations[i] ==
                     NullableAnnotation.Annotated
            ? $"{qualifiedName}?"
            : qualifiedName;
      }
    }

    private static string ConvertName_(ITypeSymbol typeSymbol,
                                       ISymbol?
                                           typeSymbolForAttributeChecks)
      => typeSymbol.HasAttribute<GenerateReadOnlyAttribute>() &&
         !(typeSymbolForAttributeChecks ?? typeSymbol)
             .HasAttribute<KeepMutableTypeAttribute>()
          ? typeSymbol.GetConstInterfaceName()
          : typeSymbol.Name.EscapeKeyword();

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

    public static IEnumerable<INamedTypeSymbol> LookupTypesWithName(
        SemanticModel semanticModel,
        TypeDeclarationSyntax syntax,
        string searchString)
      => semanticModel
         .LookupNamespacesAndTypes(syntax.SpanStart, null, searchString)
         .Where(symbol => symbol is INamedTypeSymbol)
         .Select(symbol => symbol as INamedTypeSymbol);

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

          var typesWithName
              = LookupTypesWithName(semanticModel, syntax, nameWithoutPrefix)
                  .ToArray();
          if (typesWithName.Length == 0) {
            typesWithName = LookupTypesWithName(semanticModel,
                                                syntax,
                                                $"B{nameWithoutPrefix}")
                .ToArray();
          }

          if (typesWithName.Length == 0) {
            typesWithName = LookupTypesWithName(semanticModel,
                                                syntax,
                                                $"I{nameWithoutPrefix}")
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

      var sb = new StringBuilder();
      sb.Append("<");
      for (var i = 0; i < typeParameters.Length; ++i) {
        if (i > 0) {
          sb.Append(", ");
        }

        var typeParameter = typeParameters[i];

        var variance = typeParameter.Variance;
        if (variance == VarianceKind.None) {
          variance = typeParameter.GetExpandedVarianceForReadonlyVersion(
              allParentTypes,
              constMembers);
        }

        sb.Append(variance switch {
          VarianceKind.In => "in ",
          VarianceKind.Out => "out ",
          VarianceKind.None => "",
        });

        sb.Append(typeParameter.Name.EscapeKeyword());
      }

      sb.Append(">");
      return sb.ToString();
    }

    public static VarianceKind GetExpandedVarianceForReadonlyVersion(
        this ITypeParameterSymbol typeParameterSymbol,
        IReadOnlyList<INamedTypeSymbol> allParentTypes,
        IReadOnlyList<IMethodSymbol> constMembers) {
      var matchingTypeArguments
          = allParentTypes
            .SelectMany(NamedTypeSymbolUtil.GetTypeParamsAndArgs)
            .Where(paramAndArg => paramAndArg.typeArgumentSymbol.Name ==
                                  typeParameterSymbol.Name)
            .ToArray();

      if (matchingTypeArguments.Length == 0 ||
          matchingTypeArguments.All(
              paramAndArg => paramAndArg.typeParameterSymbol.Variance ==
                             VarianceKind.Out)) {
        return constMembers.Any(constMember => constMember.Parameters.Any(
                                    p => p.Type.DependsOn(typeParameterSymbol)))
            ? VarianceKind.None
            : VarianceKind.Out;
      }

      if (matchingTypeArguments.Length == 0 ||
          matchingTypeArguments.All(
              paramAndArg => paramAndArg.typeParameterSymbol.Variance ==
                             VarianceKind.In)) {
        return constMembers.Any(
            constMember
                => constMember.ReturnType.DependsOn(typeParameterSymbol))
            ? VarianceKind.None
            : VarianceKind.In;
      }

      return VarianceKind.None;
    }

    public static bool DependsOn(this ITypeSymbol typeSymbol,
                                 ITypeSymbol otherTypeSymbol) {
      if (typeSymbol.IsSameAs(otherTypeSymbol)) {
        return true;
      }

      if (typeSymbol.IsGeneric(out var typeParameters, out _)) {
        if (typeParameters.Any(p => p.IsSameAs(typeSymbol))) {
          return true;
        }
      }

      return false;
    }
  }
}