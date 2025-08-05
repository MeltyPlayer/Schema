using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using schema.binary.parser;
using schema.readOnly;
using schema.util.generators;
using schema.util.symbols;
using schema.util.text;


namespace schema.autoInterface;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class GenerateInterfaceAttribute : Attribute;

[Generator(LanguageNames.CSharp)]
public class AutoInterfaceTypeGenerator
    : BNamedTypesWithAttributeGenerator<GenerateInterfaceAttribute> {
  private static readonly TypeInfoParser parser_ = new();

  internal override bool FilterNamedTypesBeforeGenerating(
      TypeDeclarationSyntax syntax,
      INamedTypeSymbol symbol) => true;

  internal override IEnumerable<(string fileName, string source)>
      GenerateSourcesForNamedType(INamedTypeSymbol symbol,
                                  SemanticModel semanticModel,
                                  TypeDeclarationSyntax syntax) {
    yield return ($"{symbol.GetUniqueNameForGenerator()}_interface.g",
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

    var interfaceName = typeSymbol.GetInterfaceName();
    var context = new GeneratorUtilContext(
        new Dictionary<(string name, int arity), IEnumerable<string>?> {
            [(interfaceName, typeSymbol.Arity)] = typeSymbol.GetContainingNamespaces(),
        });

    sw.WriteNamespaceAndParentTypeBlocks(
        typeSymbol,
        () => {
          // Class
          {
            var blockPrefix =
                typeSymbol.GetQualifiersAndNameAndGenericParametersFor() +
                " : " +
                typeSymbol.GetNameAndGenericParametersFor(interfaceName);
            sw.Write(blockPrefix).WriteLine(";");
          }
          sw.WriteLine("");

          // Interface
          var interfaceMembers
              = parser_
                .ParseMembers(typeSymbol)
                .Where(parsedMember => {
                  var (parseStatus, memberSymbol, _, _) = parsedMember;

                  if (memberSymbol.IsImplicitlyDeclared) {
                    return false;
                  }

                  if (memberSymbol.DeclaringSyntaxReferences.Length == 0) {
                    return false;
                  }

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

                  if (memberSymbol is IMethodSymbol {
                          AssociatedSymbol: not null
                      }) {
                    return false;
                  }

                  return true;
                })
                .Select(parsedMember => parsedMember.Item2)
                .ToArray();

          {
            sw.WriteLine("#nullable enable");
            sw.Write(
                SymbolTypeUtil.AccessibilityToModifier(
                    typeSymbol.DeclaredAccessibility));
            sw.Write(" interface ");

            var blockPrefix = interfaceName;
            blockPrefix += typeSymbol.GetGenericParametersWithVariance();
            var parentInterfaceNames =
                typeSymbol.Interfaces
                          .Select(i => typeSymbol
                                      .GetQualifiedNameAndGenericsOrReadOnlyFromCurrentSymbol(
                                          i,
                                          semanticModel,
                                          syntax))
                          .ToArray();
            if (parentInterfaceNames.Length > 0) {
              blockPrefix += " : " + string.Join(", ", parentInterfaceNames);
            }

            blockPrefix += typeSymbol.GetTypeConstraintsOrReadonly(
                typeSymbol.TypeParameters,
                semanticModel,
                syntax,
                context);

            if (interfaceMembers.Length == 0) {
              sw.Write(blockPrefix).WriteLine(";");
            } else {
              sw.EnterBlock(blockPrefix);
              WriteMembers_(sw,
                            typeSymbol,
                            interfaceMembers,
                            semanticModel,
                            syntax,
                            context);
              sw.ExitBlock();
            }
          }
        });

    return sb.ToString();
  }

  private static void WriteMembers_(
      ISourceWriter sw,
      INamedTypeSymbol typeSymbol,
      IReadOnlyList<ISymbol> constMembers,
      SemanticModel semanticModel,
      TypeDeclarationSyntax syntax,
      GeneratorUtilContext context) {
    foreach (var memberSymbol in constMembers) {
      switch (memberSymbol) {
        case IPropertySymbol propertySymbol: {
          var memberTypeSymbol = propertySymbol.Type;

          sw.Write(SymbolTypeUtil.AccessibilityToModifier(
                       typeSymbol.DeclaredAccessibility))
            .Write(" ");

          sw.Write(
                typeSymbol
                    .GetQualifiedNameAndGenericsOrReadOnlyFromCurrentSymbol(
                        memberTypeSymbol,
                        semanticModel,
                        syntax,
                        propertySymbol))
            .Write(" ");

          var isIndexer = propertySymbol.IsIndexer;
          if (!isIndexer) {
            sw.Write(memberSymbol.Name.EscapeKeyword());
          } else {
            var indexerParameterSymbols = propertySymbol.Parameters;
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

          sw.Write(" { ");
          if (propertySymbol.GetMethod != null) {
            sw.Write("get; ");
          }
          if (propertySymbol.SetMethod != null) {
            sw.Write("set; ");
          }
          sw.WriteLine("}");
          break;
        }
        case IMethodSymbol methodSymbol: {
          var memberTypeSymbol = methodSymbol.ReturnType;

          sw.Write(SymbolTypeUtil.AccessibilityToModifier(
                       typeSymbol.DeclaredAccessibility))
            .Write(" ");

          IPropertySymbol? associatedPropertySymbol
              = methodSymbol.AssociatedSymbol as IPropertySymbol;
          sw.Write(
                typeSymbol
                    .GetQualifiedNameAndGenericsOrReadOnlyFromCurrentSymbol(
                        memberTypeSymbol,
                        semanticModel,
                        syntax,
                        (ISymbol?) associatedPropertySymbol ?? memberSymbol))
            .Write(" ");

          var accessName = memberSymbol.Name.EscapeKeyword();
          sw.Write(accessName);
          sw.Write(methodSymbol.TypeParameters
                               .GetGenericParameters());
          sw.Write("(");

          for (var i = 0; i < methodSymbol.Parameters.Length; ++i) {
            if (i > 0) {
              sw.Write(", ");
            }

            var parameterSymbol = methodSymbol.Parameters[i];
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


            if (parameterSymbol.HasExplicitDefaultValue) {
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

          sw.Write(typeSymbol.GetTypeConstraintsOrReadonly(
                       methodSymbol.TypeParameters,
                       semanticModel,
                       syntax,
                       context));

          sw.WriteLine(";");
          break;
        }
        default:
          throw new NotSupportedException();
      }
    }
  }
}

internal static class InterfaceGeneratorUtil {
  public static string GetInterfaceName(
      this ITypeSymbol typeSymbol) {
    var baseName = typeSymbol.Name;
    if (baseName.Length >= 2) {
      if (baseName[1] is < 'a' or > 'z') {
        var firstChar = baseName[0];
        if (firstChar == 'B' && typeSymbol.IsAbstractClass()) {
          baseName = baseName.Substring(1);
        }
      }
    }

    return $"I{baseName}";
  }
}