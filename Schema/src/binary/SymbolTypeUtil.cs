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


namespace schema.binary {
  internal static class SymbolTypeUtil {
    public static ISymbol GetSymbolFromType(SemanticModel semanticModel,
                                            Type type)
      => SymbolTypeUtil.GetSymbolFromIdentifier(semanticModel, type.FullName);

    public static ISymbol GetSymbolFromIdentifier(
        SemanticModel semanticModel,
        string identifier) {
      var symbol = semanticModel.LookupSymbols(0, null, identifier);
      return symbol.First();
    }

    public static bool CanBeStoredAs<TType>(this ITypeSymbol symbol)
      => symbol.CanBeStoredAs(typeof(TType));

    public static bool CanBeStoredAs(this ITypeSymbol symbol, Type type) {
      if (symbol.IsExactlyType(type) || symbol.Implements(type) ||
          symbol.ImplementsGeneric(type)) {
        return true;
      }

      if (symbol is INamedTypeSymbol namedSymbol &&
          namedSymbol.MatchesGeneric(type)) {
        return true;
      }

      return false;
    }


    public static bool ImplementsGeneric(this ITypeSymbol symbol, Type type)
      => symbol.ImplementsGeneric(type, out _);

    public static bool ImplementsGeneric(this ITypeSymbol symbol,
                                         Type type,
                                         out INamedTypeSymbol outGenericType) {
      var matchingGenericType =
          symbol.AllInterfaces.FirstOrDefault(i => i.MatchesGeneric(type));
      if (matchingGenericType != null) {
        outGenericType = matchingGenericType;
        return true;
      }

      outGenericType = default;
      return false;
    }


    public static bool IsBinaryDeserializable(this ITypeSymbol symbol)
      => symbol.Implements<IBinaryDeserializable>();

    public static bool IsBinarySerializable(this ITypeSymbol symbol)
      => symbol.Implements<IBinarySerializable>();

    public static bool Implements<TType>(this ITypeSymbol symbol)
      => symbol.Implements(typeof(TType));

    public static bool Implements(this ITypeSymbol symbol, Type type)
      => symbol.AllInterfaces.Any(i => i.IsExactlyType(type));


    public static string[]? GetContainingNamespaces(this ISymbol symbol) {
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

    public static bool HasEmptyConstructor(this INamedTypeSymbol symbol)
      => symbol.InstanceConstructors.Any(c => c.Parameters.Length == 0);

    public static bool IsPartial(this TypeDeclarationSyntax syntax)
      => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));

    public static bool MatchesGeneric(this INamedTypeSymbol symbol,
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

    public static bool IsExactlyType(this ISymbol symbol, Type expectedType)
      => symbol.Name == expectedType.Name &&
         SymbolTypeUtil.MergeContainingNamespaces(symbol) ==
         expectedType.Namespace;

    internal static bool HasAttribute(this ISymbol symbol, Type expectedType)
      => symbol.GetAttributes()
               .Any(attributeData
                        => attributeData.AttributeClass!.IsExactlyType(
                            expectedType));

    internal static IEnumerable<AttributeData>
        GetAttributeData<TAttribute>(this ISymbol symbol) {
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


    internal static bool HasAttribute<TAttribute>(
        this ISymbol symbol,
        IList<Diagnostic> diagnostics)
        where TAttribute : notnull
      => symbol.GetAttribute<TAttribute>(diagnostics) != null;

    internal static TAttribute? GetAttribute<TAttribute>(
        this ISymbol symbol,
        IList<Diagnostic> diagnostics)
        where TAttribute : notnull
      => symbol.GetAttributes<TAttribute>(diagnostics)
               .SingleOrDefault();


    internal static TAttribute? GetAttribute<TAttribute>(
        IList<Diagnostic> diagnostics,
        ISymbol symbol)
        where TAttribute : notnull
      => symbol.GetAttributes<TAttribute>(diagnostics)
               .SingleOrDefault();

    internal static IEnumerable<TAttribute> GetAttributes<TAttribute>(
        this ISymbol symbol,
        IList<Diagnostic> diagnostics)
        where TAttribute : notnull
      => symbol.GetAttributeData<TAttribute>()
               .Select(attributeData => {
                 var parameters = attributeData.AttributeConstructor
                                               .Parameters;

                 // TODO: Does this still work w/ optional arguments?
                 var attributeType = typeof(TAttribute);

                 var constructor =
                     attributeType.GetConstructors()
                                  .FirstOrDefault(c => {
                                    var cParameters = c.GetParameters();
                                    if (cParameters.Length !=
                                        parameters.Length) {
                                      return false;
                                    }

                                    for (var i = 0;
                                         i < parameters.Length;
                                         ++i) {
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
        this INamedTypeSymbol structureSymbol) {
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
        this ITypeSymbol type) {
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
        this INamedTypeSymbol namedTypeSymbol) {
      var sb = new StringBuilder();
      sb.Append(namedTypeSymbol.GetSymbolQualifiers());
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

    public static string GetSymbolQualifiers(this INamedTypeSymbol typeSymbol)
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

    public static string GetQualifiedName(this ITypeSymbol typeSymbol) {
      var mergedNamespace =
          SymbolTypeUtil.MergeContainingNamespaces(typeSymbol);
      var mergedNamespaceText = mergedNamespace == null
          ? ""
          : $"{mergedNamespace}.";

      var mergedContainersText = "";
      foreach (var container in typeSymbol.GetDeclaringTypesDownward()) {
        mergedContainersText += $"{container.Name}.";
      }

      return $"{mergedNamespaceText}{mergedContainersText}{typeSymbol.Name}";
    }

    public static string GetQualifiedNameFromCurrentSymbol(
        this ITypeSymbol sourceSymbol,
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

      var currentNamespace = sourceSymbol.GetContainingNamespaces();
      var referencedNamespace = referencedSymbol.GetContainingNamespaces();

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
      foreach (var container in referencedSymbol.GetDeclaringTypesDownward()) {
        mergedContainersText += $"{container.Name}.";
      }

      return
          $"{mergedNamespaceText}{mergedContainersText}{referencedSymbol.Name}";
    }

    public static void GetMemberInStructure(
        this ITypeSymbol structureSymbol,
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