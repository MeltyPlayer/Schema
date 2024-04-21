using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
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
      var typeV2 = TypeV2.FromSymbol(typeSymbol);

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
            .Select(parsedMember => parsedMember.Item2)
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

        var blockPrefix
            = typeSymbol.GetNameAndGenericParametersFor(interfaceName);
        var parentConstNames =
            GetDirectBaseTypeAndInterfaces_(typeSymbol)
                .Where(i => i.HasAttribute<GenerateReadOnlyAttribute>() ||
                            IsTypeAlreadyConst_(i))
                .Select(i => typeV2
                            .GetQualifiedNameAndGenericsOrReadOnlyFromCurrentSymbol(
                                TypeV2.FromSymbol(i),
                                semanticModel,
                                syntax))
                .ToArray();
        if (parentConstNames.Length > 0) {
          blockPrefix += " : " + string.Join(", ", parentConstNames);
        }

        blockPrefix
            += typeSymbol.TypeParameters.GetTypeConstraintsOrReadonly(typeV2);

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
      if (typeSymbol.MatchesGeneric(typeof(IEnumerable<>))) {
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
        IReadOnlyList<ISymbol> constMembers,
        SemanticModel semanticModel,
        TypeDeclarationSyntax syntax,
        string? interfaceName = null) {
      var typeV2 = TypeV2.FromSymbol(typeSymbol);

      foreach (var memberSymbol in constMembers) {
        var methodSymbol = Asserts.AsA<IMethodSymbol>(memberSymbol);
        var memberTypeSymbol = methodSymbol.ReturnType;

        if (interfaceName == null) {
          cbsb.Write(
              SymbolTypeUtil.AccessibilityToModifier(
                  typeSymbol.DeclaredAccessibility));
          cbsb.Write(" ");
        }

        IPropertySymbol? associatedPropertySymbol
            = methodSymbol.AssociatedSymbol as IPropertySymbol;

        var memberTypeV2 = TypeV2.FromSymbol(memberTypeSymbol);
        cbsb.Write(
            typeV2.GetQualifiedNameAndGenericsOrReadOnlyFromCurrentSymbol(
                memberTypeV2,
                semanticModel,
                syntax,
                associatedPropertySymbol));
        cbsb.Write(" ");

        var memberName = memberSymbol.Name;

        if (interfaceName != null) {
          cbsb.Write(interfaceName);
          cbsb.Write(typeSymbol.GetGenericParameters());
          cbsb.Write(".");
        }

        // Property
        if (memberName.StartsWith("get_")) {
          var isIndexer = methodSymbol.AssociatedSymbol?.Name == "this[]";

          var accessName = isIndexer
              ? "this"
              : memberSymbol.Name.Substring(4).EscapeKeyword();

          if (!isIndexer) {
            cbsb.Write(memberSymbol.Name.Substring(4).EscapeKeyword());
          } else {
            cbsb.Write("this[");
            for (var i = 0; i < methodSymbol.Parameters.Length; ++i) {
              if (i > 0) {
                cbsb.Write(", ");
              }

              var parameterSymbol = methodSymbol.Parameters[i];
              var parameterTypeV2
                  = TypeV2.FromSymbol(parameterSymbol.Type);
              cbsb.Write(
                  typeV2.GetQualifiedNameAndGenericsOrReadOnlyFromCurrentSymbol(
                      parameterTypeV2,
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
            cbsb.Write(accessName);

            if (isIndexer) {
              cbsb.Write("[");
              for (var i = 0; i < methodSymbol.Parameters.Length; ++i) {
                if (i > 0) {
                  cbsb.Write(", ");
                }

                var parameterSymbol = methodSymbol.Parameters[i];
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
          cbsb.Write(methodSymbol.TypeParameters
                                 .GetGenericParameters());
          cbsb.Write("(");

          for (var i = 0; i < methodSymbol.Parameters.Length; ++i) {
            if (i > 0) {
              cbsb.Write(", ");
            }

            var parameterSymbol = methodSymbol.Parameters[i];
            var parameterTypeV2
                = TypeV2.FromSymbol(parameterSymbol.Type);

            if (parameterSymbol.IsParams) {
              cbsb.Write("params ");
            }

            var refKindString = parameterSymbol.RefKind.GetRefKindString();
            if (refKindString.Length > 0) {
              cbsb.Write(refKindString).Write(" ");
            }

            cbsb.Write(
                    typeV2
                        .GetQualifiedNameAndGenericsOrReadOnlyFromCurrentSymbol(
                            parameterTypeV2,
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
            cbsb.Write(
                methodSymbol.TypeParameters
                            .GetTypeConstraintsOrReadonly(typeV2));
          }

          if (interfaceName == null) {
            cbsb.WriteLine(";");
          } else {
            cbsb.Write(" => ")
                .Write(accessName)
                .Write(methodSymbol.TypeParameters.GetGenericParameters())
                .Write("(");
            for (var i = 0; i < methodSymbol.Parameters.Length; ++i) {
              if (i > 0) {
                cbsb.Write(", ");
              }

              var parameterSymbol = methodSymbol.Parameters[i];

              var refKindString = parameterSymbol.RefKind.GetRefKindString();
              if (refKindString.Length > 0) {
                cbsb.Write(refKindString).Write(" ");
              }

              cbsb.Write(parameterSymbol.Name.EscapeKeyword());
            }

            cbsb.WriteLine(");");
          }
        }
      }
    }

    private static IEnumerable<INamedTypeSymbol>
        GetDirectBaseTypeAndInterfaces_(
            INamedTypeSymbol symbol) {
      var baseType = symbol.BaseType;
      if (baseType != null &&
          !baseType.IsExactlyType(typeof(object)) &&
          !baseType.IsExactlyType(typeof(ValueType))) {
        yield return baseType;
      }

      var parentInterfaces
          = symbol.Interfaces
                  .Where(i => !(i.MatchesGeneric(typeof(IEquatable<>)) &&
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
        this ITypeV2 sourceSymbol,
        ITypeV2 referencedSymbol,
        SemanticModel semanticModel,
        TypeDeclarationSyntax sourceDeclarationSyntax,
        ISymbol? memberSymbol = null)
      => sourceSymbol.GetQualifiedNameFromCurrentSymbol(
          referencedSymbol,
          memberSymbol == null
              ? ConvertName_
              : !memberSymbol.HasAttribute<KeepMutableTypeAttribute>()
                  ? ConvertName_
                  : null,
          r => GetNamespaceOfType(r, semanticModel, sourceDeclarationSyntax));

    public static string GetTypeConstraintsOrReadonly(
        this IReadOnlyList<ITypeParameterSymbol> typeParameters,
        ITypeV2 sourceSymbol) {
      var sb = new StringBuilder();

      foreach (var typeParameter in typeParameters) {
        var typeConstraintNames
            = typeParameter.GetTypeConstraintNames_(sourceSymbol)
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
        this ITypeParameterSymbol typeParameter,
        ITypeV2 sourceSymbol) {
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

      if (typeParameter is
          { HasValueTypeConstraint: true, HasUnmanagedTypeConstraint: false }) {
        yield return "struct";
      }

      for (var i = 0; i < typeParameter.ConstraintTypes.Length; ++i) {
        var constraintType = typeParameter.ConstraintTypes[i];
        var constraintTypeV2 = TypeV2.FromSymbol(constraintType);
        var qualifiedName = sourceSymbol.GetQualifiedNameFromCurrentSymbol(
            constraintTypeV2,
            !typeParameter.HasAttribute<KeepMutableTypeAttribute>()
                ? ConvertName_
                : null);

        yield return typeParameter.ConstraintNullableAnnotations[i] ==
                     NullableAnnotation.Annotated
            ? $"{qualifiedName}?"
            : qualifiedName;
      }
    }

    private static string ConvertName_(ITypeV2 typeV2)
      => typeV2.HasAttribute<GenerateReadOnlyAttribute>() &&
         !typeV2.HasAttribute<KeepMutableTypeAttribute>()
          ? typeV2.GetConstInterfaceName()
          : typeV2.Name.EscapeKeyword();

    public static string GetConstInterfaceName(
        this INamedTypeSymbol typeSymbol)
      => GetConstInterfaceName(TypeV2.FromSymbol(typeSymbol));

    public static string GetConstInterfaceName(this ITypeV2 typeV2) {
      var baseName = typeV2.Name;
      if (baseName.Length >= 2) {
        if (baseName[1] is < 'a' or > 'z') {
          var firstChar = baseName[0];
          if ((firstChar == 'I' && typeV2.IsInterface) ||
              (firstChar == 'B' && typeV2.IsAbstractClass)) {
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

    public static IEnumerable<string> GetNamespaceOfType(this ITypeV2 typeV2,
      SemanticModel semanticModel,
      TypeDeclarationSyntax syntax) {
      if (!typeV2.Exists) {
        var typeName = typeV2.Name;
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

      return typeV2.NamespaceParts;
    }
  }
}