﻿using System;
using System.Collections.Generic;
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
  [AttributeUsage(AttributeTargets.Method)]
  public class ConstAttribute : Attribute;

  [AttributeUsage(AttributeTargets.Class |
                  AttributeTargets.Interface |
                  AttributeTargets.Struct)]
  public class GenerateReadOnlyAttribute : Attribute;

  [Generator(LanguageNames.CSharp)]
  public class ReadOnlyTypeGenerator
      : BNamedTypesWithAttributeGenerator<GenerateReadOnlyAttribute> {
    private static readonly TypeInfoParser parser_ = new();

    public const string PREFIX = "IReadOnly";

    internal override bool FilterNamedTypesBeforeGenerating(
        TypeDeclarationSyntax syntax,
        INamedTypeSymbol symbol) => true;

    internal override IEnumerable<(string fileName, string source)>
        GenerateSourcesForNamedType(
            INamedTypeSymbol symbol) {
      var typeV2 = TypeV2.FromSymbol(symbol);
      yield return ($"{typeV2.FullyQualifiedName}_readOnly.g",
                    this.GenerateSourceForNamedType(symbol));
    }

    public string GenerateSourceForNamedType(INamedTypeSymbol typeSymbol) {
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

      var interfaceName = GetConstInterfaceNameFor_(typeSymbol);

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
          WriteMembers_(cbsb, typeSymbol, constMembers, interfaceName);
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
            = typeSymbol.GetNameAndGenericParametersFor(interfaceName) +
              typeSymbol.TypeParameters.GetTypeConstraints(typeV2);
        var parentConstNames =
            GetDirectBaseTypeAndInterfaces_(typeSymbol)
                .Where(i => i.HasAttribute<GenerateReadOnlyAttribute>() ||
                            IsTypeAlreadyConst_(i))
                .Select(i => i.HasAttribute<GenerateReadOnlyAttribute>()
                            ? typeV2.GetQualifiedNameFromCurrentSymbol(
                                TypeV2.FromSymbol(i),
                                GetConstInterfaceNameFor_(i))
                            : typeV2.GetQualifiedNameFromCurrentSymbol(
                                TypeV2.FromSymbol(i)))
                .ToArray();
        if (parentConstNames.Length > 0) {
          blockPrefix += " : " + string.Join(", ", parentConstNames);
        }

        if (constMembers.Length == 0) {
          cbsb.Write(blockPrefix).WriteLine(";");
        } else {
          cbsb.EnterBlock(blockPrefix);
          WriteMembers_(cbsb, typeSymbol, constMembers);
          cbsb.ExitBlock();
        }

        // parent types
        foreach (var declaringType in declaringTypes) {
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

        var memberTypeV2 = TypeV2.FromSymbol(memberTypeSymbol);
        cbsb.Write(typeV2.GetQualifiedNameFromCurrentSymbol(memberTypeV2));
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
            for (var i = 0; i < methodSymbol.Parameters.Length; ++i) {
              cbsb.Write("this[");

              if (i > 0) {
                cbsb.Write(", ");
              }

              var parameterSymbol = methodSymbol.Parameters[i];
              var parameterTypeV2
                  = TypeV2.FromSymbol(parameterSymbol.Type);
              cbsb.Write(
                  typeV2.GetQualifiedNameFromCurrentSymbol(
                      parameterTypeV2));
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
            cbsb.Write(
                    typeV2.GetQualifiedNameFromCurrentSymbol(parameterTypeV2))
                .Write(" ")
                .Write(parameterSymbol.Name.EscapeKeyword());
          }

          cbsb.Write(")")
              .Write(
                  methodSymbol.TypeParameters.GetTypeConstraints(typeV2));

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
              cbsb.Write(parameterSymbol.Name.EscapeKeyword());
            }

            cbsb.WriteLine(");");
          }
        }
      }
    }

    private static string GetConstInterfaceNameFor_(
        INamedTypeSymbol typeSymbol) {
      var typeV2 = TypeV2.FromSymbol(typeSymbol);
      var baseName = typeSymbol.Name;
      if (baseName.Length >= 2) {
        if (baseName[1] is < 'a' or > 'z') {
          var firstChar = baseName[0];
          if ((firstChar == 'I' && typeV2.IsInterface) ||
              (firstChar == 'B' &&
               typeSymbol is { IsAbstract: true, TypeKind: TypeKind.Class })) {
            baseName = baseName.Substring(1);
          }
        }
      }

      return $"{PREFIX}{baseName}";
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
}