using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using schema.binary.attributes;
using schema.binary.parser;
using schema.util;
using schema.util.diagnostics;


namespace schema.binary {
  internal static class SymbolTypeUtil {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ISymbol GetSymbolFromType(SemanticModel semanticModel,
                                            Type type)
      => SymbolTypeUtil.GetSymbolFromIdentifier(semanticModel, type.FullName);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ISymbol GetSymbolFromIdentifier(
        SemanticModel semanticModel,
        string identifier) {
      var symbol = semanticModel.LookupSymbols(0, null, identifier);
      return symbol.First();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBinaryDeserializable(this ITypeSymbol symbol)
      => symbol.Implements<IBinaryDeserializable>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBinarySerializable(this ITypeSymbol symbol)
      => symbol.Implements<IBinarySerializable>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Implements<TType>(this ITypeSymbol symbol)
      => symbol.Implements(typeof(TType));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Implements(this ITypeSymbol symbol, Type type)
      => symbol.AllInterfaces.Any(i => i.IsExactlyType(type));


    // Namespace
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsInSameNamespaceAs(this ISymbol symbol, ISymbol other)
      => symbol.ContainingNamespace.Equals(other.ContainingNamespace);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsInSameNamespaceAs(this ISymbol symbol, Type other)
      => symbol.IsInNamespace(other.Namespace);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsInNamespace(
        this ISymbol symbol,
        string fullNamespacePath) {
      var fullNamespacePathLength = fullNamespacePath.Length;

      var currentNamespace = symbol.ContainingNamespace;
      var currentNamespaceName = currentNamespace.Name;
      var currentNamespaceNameLength = currentNamespaceName.Length;

      if (fullNamespacePathLength == 0 && currentNamespaceNameLength == 0) {
        return true;
      }

      var fullNamespaceI = fullNamespacePathLength - 1;
      var currentNamespaceI = currentNamespaceNameLength - 1;

      for (; fullNamespaceI >= 0; --fullNamespaceI, --currentNamespaceI) {
        if (currentNamespaceI == -1) {
          if (fullNamespacePath[fullNamespaceI] != '.') {
            return false;
          }

          --fullNamespaceI;
          currentNamespace = currentNamespace.ContainingNamespace;
          currentNamespaceName = currentNamespace.Name;
          currentNamespaceNameLength = currentNamespaceName.Length;
          if (currentNamespaceNameLength == 0) {
            return false;
          }

          currentNamespaceI = currentNamespaceNameLength - 1;
        }

        if (fullNamespacePath[fullNamespaceI] !=
            currentNamespaceName[currentNamespaceI]) {
          return false;
        }
      }

      return fullNamespaceI == -1 && currentNamespaceI == -1 &&
             currentNamespace.ContainingNamespace.Name.Length == 0;
    }

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsExactlyType(this ISymbol symbol, Type expectedType)
      => symbol.Name == expectedType.Name &&
         symbol.IsInSameNamespaceAs(expectedType);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ImmutableArray<AttributeData> GetAttributeData(
        this ISymbol symbol)
      => symbol.GetAttributes();


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasAttribute(this ISymbol symbol, Type expectedType)
      => symbol.GetAttributeData()
               .Any(attributeData
                        => attributeData.AttributeClass!.IsExactlyType(
                            expectedType));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IEnumerable<AttributeData>
        GetAttributeData<TAttribute>(this ISymbol symbol) {
      var attributeType = typeof(TAttribute);
      return symbol
             .GetAttributeData()
             .Where(attributeData
                        => attributeData.AttributeClass?.IsExactlyType(
                            attributeType) ?? false);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasAttribute<TAttribute>(this ISymbol symbol)
        where TAttribute : Attribute
      => symbol.GetAttributeData<TAttribute>().Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static TAttribute? GetAttribute<TAttribute>(
        this ISymbol symbol,
        IDiagnosticReporter diagnosticReporter)
        where TAttribute : Attribute
      => symbol.GetAttributes<TAttribute>(diagnosticReporter)
               .SingleOrDefault();


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static TAttribute? GetAttribute<TAttribute>(
        IDiagnosticReporter diagnosticReporter,
        ISymbol symbol)
        where TAttribute : Attribute
      => symbol.GetAttributes<TAttribute>(diagnosticReporter)
               .SingleOrDefault();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IEnumerable<TAttribute> GetAttributes<TAttribute>(
        this ISymbol symbol,
        IDiagnosticReporter diagnosticReporter)
        where TAttribute : Attribute
      => symbol.GetAttributeData<TAttribute>()
               .Select(attributeData => {
                 var attribute = attributeData.Instantiate<TAttribute>(symbol);
                 if (attribute is BMemberAttribute memberAttribute) {
                   memberAttribute.Init(diagnosticReporter,
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetSymbolQualifiers(this INamedTypeSymbol typeSymbol)
      => (typeSymbol.IsStatic ? "static " : "") +
         SymbolTypeUtil.AccessibilityToModifier(
             typeSymbol.DeclaredAccessibility) +
         " " +
         (typeSymbol.IsAbstract ? "abstract " : "") +
         "partial " +
         (typeSymbol.TypeKind == TypeKind.Class ? "class" : "struct");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        IDiagnosticReporter diagnosticReporter,
        ITypeSymbol structureSymbol,
        string otherMemberName,
        string thisMemberNameForFirstPass,
        bool assertOrder,
        out ISymbol memberSymbol,
        out ITypeInfo memberTypeInfo) {
      var typeChain = AccessChainUtil.GetAccessChainForRelativeMember(
          diagnosticReporter,
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