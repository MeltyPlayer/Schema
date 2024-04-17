using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;

using schema.binary.parser;
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
      : BNamedTypeGenerator<GenerateReadOnlyAttribute> {
    public const string PREFIX = "IReadOnly";

    internal override bool Generate(
        INamedTypeSymbol typeSymbol,
        out string fileName,
        out string source) {
      var typeV2 = TypeV2.FromSymbol(typeSymbol);
      fileName = $"{typeV2.FullyQualifiedName}_readOnly.g";

      return this.Generate(typeSymbol, out source);
    }

    public bool Generate(INamedTypeSymbol typeSymbol,
                         out string source) {
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

      var interfaceName = this.GetConstInterfaceNameFor_(typeSymbol);

      // Class
      {
        cbsb.WriteLine(
            typeSymbol.GetQualifiersAndNameAndGenericParametersFor() +
            " : " +
            typeSymbol.GetNameAndGenericParametersFor(interfaceName) +
            ";");
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
            this
                .GetDirectBaseTypeAndInterfaces_(typeSymbol)
                .Where(i => i.HasAttribute<GenerateReadOnlyAttribute>())
                .Select(i => this.GetConstInterfaceNameFor_(i) +
                             i.TypeArguments.GetGenericArguments(typeV2))
                .ToArray();
        if (parentConstNames.Length > 0) {
          blockPrefix += " : " + string.Join(", ", parentConstNames);
        }

        cbsb.EnterBlock(blockPrefix);

        foreach (var parsedMember in new TypeInfoParser().ParseMembers(
                     typeSymbol)) {
          var (parseStatus, memberSymbol, memberTypeSymbol, memberTypeInfo)
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

          if (memberSymbol is IPropertySymbol { IsWriteOnly: true }) {
            continue;
          }

          if (memberSymbol is IMethodSymbol &&
              !memberSymbol.HasAttribute<ConstAttribute>()) {
            continue;
          }

          {
            if (memberSymbol is IMethodSymbol methodSymbol) {
              memberTypeSymbol = methodSymbol.ReturnType;
            }
          }

          cbsb.Write(
              SymbolTypeUtil.AccessibilityToModifier(
                  typeSymbol.DeclaredAccessibility));
          cbsb.Write(" ");

          var memberTypeV2 = TypeV2.FromSymbol(memberTypeSymbol);
          cbsb.Write(typeV2.GetQualifiedNameFromCurrentSymbol(memberTypeV2));
          cbsb.Write(" ");

          cbsb.Write(memberSymbol.Name.EscapeKeyword());

          switch (memberSymbol) {
            case IMethodSymbol methodSymbol: {
              cbsb.Write(methodSymbol.TypeParameters.GetGenericParameters());
              cbsb.Write("(");

              for (var i = 0; i < methodSymbol.Parameters.Length; ++i) {
                if (i > 0) {
                  cbsb.Write(", ");
                }

                var parameterSymbol = methodSymbol.Parameters[i];
                var parameterTypeV2 = TypeV2.FromSymbol(parameterSymbol.Type);
                cbsb.Write(
                    typeV2.GetQualifiedNameFromCurrentSymbol(parameterTypeV2));
                cbsb.Write(" ");
                cbsb.Write(parameterSymbol.Name.EscapeKeyword());
              }

              cbsb.Write(")");
              cbsb.Write(
                  methodSymbol.TypeParameters.GetTypeConstraints(typeV2));

              cbsb.WriteLine(";");
              break;
            }
            case IPropertySymbol: {
              cbsb.WriteLine(" { get; }");
              break;
            }
          }
        }

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

      source = sb.ToString();
      return true;
    }

    private string GetConstInterfaceNameFor_(INamedTypeSymbol typeSymbol) {
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

    private IEnumerable<INamedTypeSymbol> GetDirectBaseTypeAndInterfaces_(
        INamedTypeSymbol symbol) {
      if (symbol.BaseType != null) {
        yield return symbol.BaseType;
      }

      foreach (var iface in symbol.Interfaces) {
        yield return iface;
      }
    }
  }
}