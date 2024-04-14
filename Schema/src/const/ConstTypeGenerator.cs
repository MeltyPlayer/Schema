using System;
using System.IO;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using schema.binary.parser;
using schema.util.generators;
using schema.util.symbols;
using schema.util.text;
using schema.util.types;

namespace schema.@const {
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
  public class ConstAttribute : Attribute;

  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
  public class GenerateConstAttribute : Attribute;

  public class ConstTypeGenerator : BNamedTypeGenerator {
    public const string PREFIX = "IConst";

    internal override void Generate(
        TypeDeclarationSyntax syntax,
        INamedTypeSymbol typeSymbol,
        ISourceFileDictionary sourceFileDictionary) {
      var typeV2 = TypeV2.FromSymbol(typeSymbol);
      if (this.Generate(syntax, typeSymbol, out var source)) {
        sourceFileDictionary.Add($"{typeV2.FullyQualifiedName}_const.g",
                                 source);
      }
    }

    public bool Generate(TypeDeclarationSyntax syntax,
                           INamedTypeSymbol typeSymbol,
                           out string source) {
      var typeV2 = TypeV2.FromSymbol(typeSymbol);

      if (!typeV2.HasAttribute<GenerateConstAttribute>()) {
        source = default;
        return false;
      }

      var typeNamespace = typeSymbol.GetFullyQualifiedNamespace();

      var declaringTypes = typeSymbol.GetDeclaringTypesDownward();

      var sb = new StringBuilder();
      using var cbsb = new CurlyBracketTextWriter(new StringWriter(sb));

      // TODO: Handle fancier cases here
      if (typeNamespace != null) {
        cbsb.EnterBlock($"namespace {typeNamespace}");
      }

      foreach (var declaringType in declaringTypes) {
        cbsb.EnterBlock(declaringType.GetQualifiersAndNameAndGenericsFor());
      }

      // Interface
      {
        cbsb.EnterBlock(typeSymbol.GetQualifiersAndNameAndGenericsFor(PREFIX));

        foreach (var parsedMember in new TypeInfoParser().ParseMembers(
                     typeSymbol)) {
          var (parseStatus, memberSymbol, memberTypeSymbol, memberTypeInfo) = parsedMember;
          if (parseStatus ==
              TypeInfoParser.ParseStatus.NOT_A_FIELD_OR_PROPERTY_OR_METHOD) {
            continue;
          }

          if (memberSymbol.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal)) {
            continue;
          }

          if (memberSymbol is IFieldSymbol) {
            continue;
          }

          if (memberSymbol is IPropertySymbol { IsWriteOnly: true }) {
            continue;
          }

          if (!memberSymbol.HasAttribute<ConstAttribute>()) {
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

          cbsb.Write(memberSymbol.Name);

          switch (memberSymbol) {
            case IMethodSymbol methodSymbol: {
              cbsb.Write(methodSymbol.TypeParameters.GetGenerics());
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
                cbsb.Write(parameterSymbol.Name);
              }

              cbsb.WriteLine(");");
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
      cbsb.WriteLine("");

      // Class
      {
        cbsb.EnterBlock(
            typeSymbol.GetQualifiersAndNameAndGenericsFor() +
            " : " +
            typeSymbol.GetNameAndGenericsFor(PREFIX));

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
  }
}