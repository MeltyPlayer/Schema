using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using schema.binary.attributes;
using schema.binary.parser;
using schema.binary.util;


namespace schema.binary {
  internal static class SymbolTypeUtil {
    public static ISymbol GetSymbolFromType(SemanticModel semanticModel,
                                            Type type)
      => GetSymbolFromIdentifier(semanticModel, type.FullName);

    public static ISymbol GetSymbolFromIdentifier(
        SemanticModel semanticModel,
        string identifier) {
      var symbol = semanticModel.LookupSymbols(0, null, identifier);
      return symbol.First();
    }

    public static bool CanBeStoredAs(ITypeSymbol symbol, Type type) {
      if (IsExactlyType(symbol, type) ||
          Implements(symbol, type) ||
          ImplementsGeneric(symbol, type)) {
        return true;
      }

      if (symbol is INamedTypeSymbol namedSymbol &&
          MatchesGeneric(namedSymbol, type)) {
        return true;
      }

      return false;
    }

    public static bool ImplementsGeneric(ITypeSymbol symbol, Type type)
      => symbol.AllInterfaces.Any(i => SymbolTypeUtil.MatchesGeneric(i, type));

    public static bool Implements(ITypeSymbol symbol, Type type)
      => symbol.AllInterfaces.Any(i => SymbolTypeUtil.IsExactlyType(i, type));

    public static string[]? GetContainingNamespaces(ISymbol symbol) {
      var namespaceSymbol = symbol.ContainingNamespace;
      if (namespaceSymbol == null) {
        return null;
      }

      var namespaces = new LinkedList<string>();
      while (namespaceSymbol != null) {
        if (namespaceSymbol.Name.Length > 0) {
          namespaces.AddFirst(namespaceSymbol.Name);
        }

        namespaceSymbol = namespaceSymbol.ContainingNamespace;
      }

      return namespaces.ToArray();
    }

    public static string? MergeContainingNamespaces(ISymbol symbol)
      => MergeNamespaceParts(GetContainingNamespaces(symbol));

    public static string? MergeNamespaceParts(IList<string>? namespaces) {
      if ((namespaces?.Count ?? 0) == 0) {
        return null;
      }

      var combined = new StringBuilder();
      foreach (var space in namespaces) {
        if (combined.Length == 0) {
          combined.Append(space);
        } else {
          combined.Append(".");
          combined.Append(space);
        }
      }

      return combined.ToString();
    }

    public static bool HasEmptyConstructor(INamedTypeSymbol symbol)
      => symbol.InstanceConstructors.Any(c => c.Parameters.Length == 0);

    public static bool IsPartial(TypeDeclarationSyntax syntax)
      => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));

    public static bool MatchesGeneric(INamedTypeSymbol symbol,
                                      Type expectedGenericType) {
      var indexOfBacktick = expectedGenericType.Name.IndexOf('`');
      if (indexOfBacktick == -1) {
        return false;
      }

      var sameName = symbol.Name ==
                     expectedGenericType.Name.Substring(0, indexOfBacktick);
      var sameNamespace =
          SymbolTypeUtil.MergeContainingNamespaces(symbol) ==
          expectedGenericType.Namespace;
      var sameTypeArguments = symbol.TypeArguments.Length ==
                              expectedGenericType.GetTypeInfo()
                                                 .GenericTypeParameters.Length;
      return sameName && sameNamespace && sameTypeArguments;
    }

    public static bool IsExactlyType(ISymbol symbol, Type expectedType)
      => symbol.Name == expectedType.Name &&
         SymbolTypeUtil.MergeContainingNamespaces(symbol) ==
         expectedType.Namespace;

    internal static bool HasAttribute(ISymbol symbol, Type expectedType)
      => symbol.GetAttributes()
               .Any(attributeData
                        => SymbolTypeUtil.IsExactlyType(
                            attributeData.AttributeClass!,
                            expectedType));

    internal static IEnumerable<AttributeData>
        GetAttributeData<TAttribute>(ISymbol symbol) {
      var attributeType = typeof(TAttribute);
      return symbol.GetAttributes()
                   .Where(attributeData => {
                     var attributeSymbol = attributeData.AttributeClass;

                     return attributeSymbol.Name == attributeType.Name &&
                            SymbolTypeUtil.MergeContainingNamespaces(
                                attributeSymbol) ==
                            attributeType.Namespace;
                   });
    }

    internal static TAttribute? GetAttribute<TAttribute>(
        IList<Diagnostic> diagnostics,
        ISymbol symbol)
        where TAttribute : notnull
      => GetAttributes<TAttribute>(diagnostics, symbol)
          .SingleOrDefault();

    internal static IEnumerable<TAttribute> GetAttributes<TAttribute>(
        IList<Diagnostic> diagnostics,
        ISymbol symbol)
        where TAttribute : notnull
      => GetAttributeData<TAttribute>(symbol)
          .Select(attributeData => {
            var parameters = attributeData.AttributeConstructor.Parameters;

            // TODO: Does this still work w/ optional arguments?
            var attributeType = typeof(TAttribute);

            var constructor =
                attributeType.GetConstructors()
                             .FirstOrDefault(c => {
                               var cParameters = c.GetParameters();
                               if (cParameters.Length != parameters.Length) {
                                 return false;
                               }

                               for (var i = 0; i < parameters.Length; ++i) {
                                 if (parameters[i].Name !=
                                     cParameters[i].Name) {
                                   return false;
                                 }
                               }

                               return true;
                             });
            if (constructor == null) {
              throw new Exception(
                  $"Failed to find constructor for {typeof(TAttribute)}");
            }

            var arguments = attributeData.ConstructorArguments;

            var attribute = (TAttribute) constructor.Invoke(
                arguments.Select(a => a.Value).ToArray());
            if (attribute is BMemberAttribute memberAttribute) {
              memberAttribute.Init(diagnostics,
                                   symbol.ContainingType,
                                   symbol.Name);
            }

            return attribute;
          });

    public static IEnumerable<ISymbol> GetInstanceMembers(
        INamedTypeSymbol structureSymbol) {
      var baseClassesAndSelf = new LinkedList<INamedTypeSymbol>();
      {
        var currentSymbol = structureSymbol;
        while (currentSymbol != null) {
          baseClassesAndSelf.AddFirst(currentSymbol);
          currentSymbol = currentSymbol.BaseType;
        }
      }

      foreach (var currentSymbol in baseClassesAndSelf) {
        foreach (var memberSymbol in currentSymbol.GetMembers()) {
          // Skips static/const fields
          if (memberSymbol.IsStatic) {
            continue;
          }

          // Skips backing field, these are used internally for properties
          if (memberSymbol.Name.Contains("k__BackingField")) {
            continue;
          }

          // Skips indexers.
          if (memberSymbol is IPropertySymbol { IsIndexer: true }) {
            continue;
          }

          yield return memberSymbol;
        }
      }
    }

    public static INamedTypeSymbol[] GetDeclaringTypesDownward(
        ITypeSymbol type) {
      var declaringTypes = new List<INamedTypeSymbol>();

      var declaringType = type.ContainingType;
      while (declaringType != null) {
        declaringTypes.Add(declaringType);
        declaringType = declaringType.ContainingType;
      }

      declaringTypes.Reverse();

      return declaringTypes.ToArray();
    }

    public static string GetQualifiersAndNameFor(
        INamedTypeSymbol namedTypeSymbol) {
      var sb = new StringBuilder();
      sb.Append(SymbolTypeUtil.GetSymbolQualifiers(namedTypeSymbol));
      sb.Append(" ");
      sb.Append(namedTypeSymbol.Name);

      var typeParameters = namedTypeSymbol.TypeParameters;
      if (typeParameters.Length > 0) {
        sb.Append("<");
        for (var i = 0; i < typeParameters.Length; ++i) {
          if (i > 0) {
            sb.Append(", ");
          }

          var typeParameter = typeParameters[i];
          sb.Append(typeParameter.Name);
        }

        sb.Append(">");
      }

      return sb.ToString();
    }

    public static string GetSymbolQualifiers(INamedTypeSymbol typeSymbol)
      => (typeSymbol.IsStatic ? "static " : "") +
         SymbolTypeUtil.AccessibilityToModifier(
             typeSymbol.DeclaredAccessibility) +
         " " +
         (typeSymbol.IsAbstract ? "abstract " : "") +
         "partial " +
         (typeSymbol.TypeKind == TypeKind.Class ? "class" : "struct");

    public static string AccessibilityToModifier(
        Accessibility accessibility)
      => accessibility switch {
          Accessibility.Private   => "private",
          Accessibility.Protected => "protected",
          Accessibility.Internal  => "internal",
          Accessibility.Public    => "public",
          _ => throw new ArgumentOutOfRangeException(
              nameof(accessibility),
              accessibility,
              null)
      };

    public static string GetQualifiedName(ITypeSymbol typeSymbol) {
      var mergedNamespace =
          SymbolTypeUtil.MergeContainingNamespaces(typeSymbol);
      var mergedNamespaceText = mergedNamespace == null
          ? ""
          : $"{mergedNamespace}.";

      var mergedContainersText = "";
      foreach (var container in SymbolTypeUtil.GetDeclaringTypesDownward(
                   typeSymbol)) {
        mergedContainersText += $"{container.Name}.";
      }

      return $"{mergedNamespaceText}{mergedContainersText}{typeSymbol.Name}";
    }

    public static string GetQualifiedNameFromCurrentSymbol(
        ITypeSymbol sourceSymbol,
        ITypeSymbol referencedSymbol) {
      if (referencedSymbol.SpecialType
          is SpecialType.System_Byte
             or SpecialType.System_SByte
             or SpecialType.System_Int16
             or SpecialType.System_UInt16
             or SpecialType.System_Int32
             or SpecialType.System_UInt32
             or SpecialType.System_Int64
             or SpecialType.System_UInt64
             or SpecialType.System_Single
             or SpecialType.System_Double
             or SpecialType.System_Char
             or SpecialType.System_String
             or SpecialType.System_Boolean
         ) {
        return referencedSymbol.ToDisplayString();
      }

      var currentNamespace =
          SymbolTypeUtil.GetContainingNamespaces(sourceSymbol);
      var referencedNamespace =
          SymbolTypeUtil.GetContainingNamespaces(referencedSymbol);

      string mergedNamespaceText;
      if (currentNamespace == null && referencedNamespace == null) {
        mergedNamespaceText = "";
      } else if (currentNamespace == null) {
        mergedNamespaceText = $"{referencedNamespace!}.";
      } else if (referencedNamespace == null) {
        mergedNamespaceText = $"{currentNamespace}.";
      } else {
        var namespaces = new List<string>();
        var matching = true;
        for (var i = 0; i < referencedNamespace.Length; ++i) {
          if (i >= currentNamespace.Length ||
              referencedNamespace[i] != currentNamespace[i]) {
            matching = false;
          }

          if (!matching) {
            namespaces.Add(referencedNamespace[i]);
          }
        }

        mergedNamespaceText = namespaces.Count > 0
            ? $"{MergeNamespaceParts(namespaces)}."
            : "";
      }

      var mergedContainersText = "";
      foreach (var container in SymbolTypeUtil.GetDeclaringTypesDownward(
                   referencedSymbol)) {
        mergedContainersText += $"{container.Name}.";
      }

      return
          $"{mergedNamespaceText}{mergedContainersText}{referencedSymbol.Name}";
    }

    public static void GetMemberInStructure(
        ITypeSymbol structureSymbol,
        string memberName,
        out ISymbol memberSymbol,
        out ITypeInfo memberTypeInfo
    ) {
      memberSymbol = structureSymbol.GetMembers(memberName).Single();
      new TypeInfoParser().ParseMember(memberSymbol, out memberTypeInfo);
    }

    public static void GetMemberRelativeToAnother(
        IList<Diagnostic> diagnostics,
        ITypeSymbol structureSymbol,
        string otherMemberName,
        string thisMemberNameForFirstPass,
        bool assertOrder,
        out ISymbol memberSymbol,
        out ITypeInfo memberTypeInfo) {
      var typeChain = AccessChainUtil.GetAccessChainForRelativeMember(
          diagnostics,
          structureSymbol,
          otherMemberName,
          thisMemberNameForFirstPass,
          assertOrder);

      var target = typeChain.Target;
      memberSymbol = target.MemberSymbol;
      memberTypeInfo = target.MemberTypeInfo;
    }
  }
}