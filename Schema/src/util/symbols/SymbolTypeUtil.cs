using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

using Microsoft.CodeAnalysis;

using schema.binary;
using schema.binary.attributes;
using schema.binary.parser;
using schema.util.diagnostics;
using schema.util.types;


namespace schema.util.symbols {
  internal static class SymbolTypeUtil {
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


    public static string GetFullyQualifiedNamespace(this ISymbol symbol)
      => string.Join(".", symbol.GetContainingNamespaces());

    public static IEnumerable<string> GetContainingNamespaces(
        this ISymbol symbol)
      => symbol.GetContainingNamespacesReversed_().Reverse();

    private static IEnumerable<string> GetContainingNamespacesReversed_(
        this ISymbol symbol) {
      var namespaceSymbol = symbol.ContainingNamespace;
      if (namespaceSymbol?.IsGlobalNamespace ?? true) {
        yield break;
      }

      while (!namespaceSymbol?.IsGlobalNamespace ?? true) {
        if (namespaceSymbol.Name.Length > 0) {
          yield return namespaceSymbol.Name;
        }

        namespaceSymbol = namespaceSymbol.ContainingNamespace;
      }
    }
    
    public static bool MatchesGeneric(this INamedTypeSymbol symbol,
                                      Type expectedGenericType) {
      var indexOfBacktick = expectedGenericType.Name.IndexOf('`');
      if (indexOfBacktick == -1) {
        return false;
      }

      var sameName = symbol.Name ==
                     expectedGenericType.Name.Substring(0, indexOfBacktick);
      var sameNamespace = symbol.GetFullyQualifiedNamespace() ==
                          expectedGenericType.Namespace;
      var sameTypeArguments = symbol.TypeArguments.Length ==
                              expectedGenericType.GetTypeInfo()
                                                 .GenericTypeParameters.Length;
      return sameName && sameNamespace && sameTypeArguments;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsExactlyType(this ISymbol symbol, Type expectedType) {
      var expectedName = expectedType.Name;

      int expectedArity = 0;
      var indexOfBacktick = expectedName.IndexOf('`');
      if (indexOfBacktick != -1) {
        expectedArity = int.Parse(expectedName.Substring(indexOfBacktick + 1));
        expectedName = expectedName.Substring(0, indexOfBacktick);
      }

      return symbol.Name == expectedName &&
             symbol.IsInSameNamespaceAs(expectedType) &&
             ((symbol as INamedTypeSymbol)?.Arity ?? 0) == expectedArity;
    }


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
        IDiagnosticReporter? diagnosticReporter,
        ISymbol symbol)
        where TAttribute : Attribute
      => symbol.GetAttributes<TAttribute>(diagnosticReporter)
               .SingleOrDefault();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IEnumerable<TAttribute> GetAttributes<TAttribute>(
        this ISymbol symbol,
        IDiagnosticReporter? diagnosticReporter = null)
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
        this INamedTypeSymbol containerSymbol) {
      var baseClassesAndSelf = new LinkedList<INamedTypeSymbol>();
      {
        var currentSymbol = containerSymbol;
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

    public static string GetFullyQualifiedName(this ITypeSymbol typeSymbol) {
      var mergedNamespace = typeSymbol.GetFullyQualifiedNamespace();
      var mergedNamespaceText = mergedNamespace.Length == 0
          ? ""
          : $"{mergedNamespace}.";

      var mergedContainersText = "";
      foreach (var container in typeSymbol.GetDeclaringTypesDownward()) {
        mergedContainersText += $"{container.Name}.";
      }

      return $"{mergedNamespaceText}{mergedContainersText}{typeSymbol.Name}";
    }

    public static string GetQualifiedNameFromCurrentSymbol(
        this ITypeV2 sourceSymbol,
        ITypeV2 referencedSymbol) {
      if (referencedSymbol.IsPrimitive(out var primitiveType) &&
          !referencedSymbol.IsEnum(out _)) {
        // TODO: Is there a built-in for this?
        return primitiveType switch {
            SchemaPrimitiveType.BOOLEAN => "boolean",
            SchemaPrimitiveType.SBYTE   => "sbyte",
            SchemaPrimitiveType.BYTE    => "byte",
            SchemaPrimitiveType.INT16   => "short",
            SchemaPrimitiveType.UINT16  => "ushort",
            SchemaPrimitiveType.INT32   => "int",
            SchemaPrimitiveType.UINT32  => "uint",
            SchemaPrimitiveType.INT64   => "long",
            SchemaPrimitiveType.UINT64  => "ulong",
            SchemaPrimitiveType.SINGLE  => "single",
            SchemaPrimitiveType.DOUBLE  => "double",
            SchemaPrimitiveType.CHAR    => "char",
        };
      }

      if (referencedSymbol.IsString) {
        return "string";
      }

      var currentNamespace = sourceSymbol.NamespaceParts.ToArray();
      var referencedNamespace = referencedSymbol.NamespaceParts.ToArray();

      string mergedNamespaceText;
      if (currentNamespace.Length == 0 && referencedNamespace.Length == 0) {
        mergedNamespaceText = "";
      } else if (currentNamespace.Length == 0) {
        mergedNamespaceText = $"{referencedNamespace!}.";
      } else if (referencedNamespace.Length == 0) {
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
            ? $"{string.Join(".", namespaces)}."
            : "";
      }

      var mergedContainersText = "";
      foreach (var container in referencedSymbol.DeclaringTypeNamesDownward) {
        mergedContainersText += $"{container}.";
      }

      return
          $"{mergedNamespaceText}{mergedContainersText}{referencedSymbol.Name}";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetMemberInContainer(
        this ITypeSymbol containerSymbol,
        string memberName,
        out ISymbol memberSymbol,
        out ITypeSymbol memberTypeSymbol,
        out ITypeInfo memberTypeInfo
    ) {
      memberSymbol = containerSymbol.GetMembers(memberName).Single();
      new TypeInfoParser().ParseMember(memberSymbol,
                                       out memberTypeSymbol,
                                       out memberTypeInfo);
    }

    public static void GetMemberRelativeToAnother(
        IDiagnosticReporter? diagnosticReporter,
        INamedTypeSymbol containerTypeSymbol,
        string otherMemberName,
        string thisMemberNameForFirstPass,
        bool assertOrder,
        out ISymbol memberSymbol,
        out ITypeSymbol memberTypeSymbol,
        out ITypeInfo memberTypeInfo) {
      var typeChain = AccessChainUtil.GetAccessChainForRelativeMember(
          diagnosticReporter,
          containerTypeSymbol,
          otherMemberName,
          thisMemberNameForFirstPass,
          assertOrder);

      var target = typeChain.Target;
      memberSymbol = target.MemberSymbol;
      memberTypeSymbol = target.MemberTypeSymbol;
      memberTypeInfo = target.MemberTypeInfo;
    }
  }
}