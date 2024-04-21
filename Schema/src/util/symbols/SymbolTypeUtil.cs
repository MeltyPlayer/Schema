using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Microsoft.CodeAnalysis;

using schema.binary;
using schema.binary.attributes;
using schema.binary.parser;
using schema.util.diagnostics;
using schema.util.types;


namespace schema.util.symbols {
  public static class SymbolTypeUtil {
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

      return fullNamespaceI == -1 &&
             currentNamespaceI == -1 &&
             currentNamespace.ContainingNamespace.Name.Length == 0;
    }


    public static string? GetFullyQualifiedNamespace(this ISymbol symbol) {
      var containingNamespaces = symbol.GetContainingNamespaces().ToArray();
      if (containingNamespaces.Length == 0) {
        return null;
      }

      return string.Join(".", containingNamespaces);
    }

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
          yield return namespaceSymbol.Name.EscapeKeyword();
        }

        namespaceSymbol = namespaceSymbol.ContainingNamespace;
      }
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
    internal static IEnumerable<AttributeData>
        GetAttributeData<TAttribute>(this ISymbol symbol) {
      var attributeType = typeof(TAttribute);
      return symbol
             .GetAttributes()
             .Where(attributeData
                        => attributeData.AttributeClass?.IsExactlyType(
                               attributeType) ??
                           false);
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
                         var attribute
                             = attributeData.Instantiate<TAttribute>(symbol);
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

    public static string GetQualifiersAndNameAndGenericParametersFor(
        this INamedTypeSymbol namedTypeSymbol,
        string? replacementName = null)
      => new StringBuilder()
         .AppendQualifiersAndNameAndGenericParametersFor(
             namedTypeSymbol,
             replacementName)
         .ToString();

    public static StringBuilder AppendQualifiersAndNameAndGenericParametersFor(
        this StringBuilder sb,
        INamedTypeSymbol namedTypeSymbol,
        string? replacementName = null)
      => sb.AppendSymbolQualifiers(namedTypeSymbol)
           .Append(" ")
           .AppendNameAndGenericParametersFor(namedTypeSymbol, replacementName);

    public static string GetNameAndGenericParametersFor(
        this INamedTypeSymbol namedTypeSymbol,
        string? replacementName = null)
      => new StringBuilder()
         .AppendNameAndGenericParametersFor(namedTypeSymbol, replacementName)
         .ToString();

    public static StringBuilder AppendNameAndGenericParametersFor(
        this StringBuilder sb,
        INamedTypeSymbol namedTypeSymbol,
        string? replacementName = null)
      => sb.Append(replacementName ?? namedTypeSymbol.Name.EscapeKeyword())
           .AppendGenericParametersFor(namedTypeSymbol);

    public static string GetGenericParameters(
        this INamedTypeSymbol namedTypeSymbol)
      => new StringBuilder()
         .AppendGenericParametersFor(namedTypeSymbol)
         .ToString();

    public static StringBuilder AppendGenericParametersFor(
        this StringBuilder sb,
        INamedTypeSymbol namedTypeSymbol)
      => sb.AppendGenericParameters(namedTypeSymbol.TypeParameters);

    public static string GetGenericParameters(
        this IReadOnlyList<ITypeParameterSymbol> typeParameters)
      => new StringBuilder()
         .AppendGenericParameters(typeParameters)
         .ToString();

    public static StringBuilder AppendGenericParameters(
        this StringBuilder sb,
        IReadOnlyList<ITypeParameterSymbol> typeParameters) {
      if (typeParameters.Count > 0) {
        sb.Append("<");
        for (var i = 0; i < typeParameters.Count; ++i) {
          if (i > 0) {
            sb.Append(", ");
          }

          var typeParameter = typeParameters[i];
          sb.Append(typeParameter.Name.EscapeKeyword());
        }

        sb.Append(">");
      }

      return sb;
    }

    public static string GetGenericArguments(
        this IReadOnlyList<ITypeSymbol> typeArguments,
        ITypeV2 sourceSymbol)
      => new StringBuilder()
         .AppendGenericArguments(typeArguments, sourceSymbol)
         .ToString();

    public static StringBuilder AppendGenericArguments(
        this StringBuilder sb,
        IReadOnlyList<ITypeSymbol> typeArguments,
        ITypeV2 sourceSymbol) {
      if (typeArguments.Count > 0) {
        sb.Append("<");
        for (var i = 0; i < typeArguments.Count; ++i) {
          if (i > 0) {
            sb.Append(", ");
          }

          var typeArgument = typeArguments[i];
          var typeV2 = TypeV2.FromSymbol(typeArgument);
          sb.Append(sourceSymbol
                        .GetQualifiedNameFromCurrentSymbol(typeV2));
        }

        sb.Append(">");
      }

      return sb;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StringBuilder AppendSymbolQualifiers(
        this StringBuilder sb,
        INamedTypeSymbol typeSymbol)
      => sb.Append(typeSymbol.IsStatic ? "static " : "")
           .Append(SymbolTypeUtil.AccessibilityToModifier(
                       typeSymbol.DeclaredAccessibility))
           .Append(" ")
           .Append(typeSymbol is { IsAbstract: true, TypeKind: TypeKind.Class }
                       ? "abstract "
                       : "")
           .Append("partial ")
           .Append(typeSymbol.TypeKind switch {
               TypeKind.Class => typeSymbol.IsRecord ? "record" : "class",
               TypeKind.Struct => typeSymbol.IsRecord
                   ? "record struct"
                   : "struct",
               TypeKind.Interface => "interface"
           });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string AccessibilityToModifier(Accessibility accessibility)
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

    public static string GetRefKindString(this RefKind refKind)
      => refKind switch {
          RefKind.In  => "in",
          RefKind.Out => "out",
          RefKind.Ref => "ref",
          _           => "",
      };

    public static string GetQualifiedNameFromCurrentSymbol(
        this ITypeV2 sourceSymbol,
        ITypeV2 referencedSymbol,
        Func<ITypeV2, string>? convertName = null,
        Func<ITypeV2, IEnumerable<string>>? getNamespaceParts = null) {
      if (referencedSymbol.IsArray(out var elementType)) {
        return
            $"{sourceSymbol.GetQualifiedNameFromCurrentSymbol(elementType, convertName, getNamespaceParts)}[]";
      }

      if (referencedSymbol.HasNullableAnnotation) {
        if (referencedSymbol is
            { Name: "Nullable", FullyQualifiedNamespace: "System" }) {
          return
              $"{sourceSymbol.GetQualifiedNameFromCurrentSymbol(referencedSymbol.GenericArguments.Single(), convertName, getNamespaceParts)}?";
        }
      }

      if (referencedSymbol.IsPrimitive(out var primitiveType) &&
          !referencedSymbol.IsEnum(out _)) {
        // TODO: Is there a built-in for this?
        return primitiveType switch {
            SchemaPrimitiveType.BOOLEAN => "bool",
            SchemaPrimitiveType.SBYTE   => "sbyte",
            SchemaPrimitiveType.BYTE    => "byte",
            SchemaPrimitiveType.INT16   => "short",
            SchemaPrimitiveType.UINT16  => "ushort",
            SchemaPrimitiveType.INT32   => "int",
            SchemaPrimitiveType.UINT32  => "uint",
            SchemaPrimitiveType.INT64   => "long",
            SchemaPrimitiveType.UINT64  => "ulong",
            SchemaPrimitiveType.SINGLE  => "float",
            SchemaPrimitiveType.DOUBLE  => "double",
            SchemaPrimitiveType.CHAR    => "char",
        };
      }

      if (referencedSymbol.IsString) {
        return "string";
      }

      if (referencedSymbol.IsGenericTypeParameter(out _)) {
        return referencedSymbol.Name.EscapeKeyword();
      }

      if (referencedSymbol is
          { Name: "Void", FullyQualifiedNamespace: "System" }) {
        return "void";
      }


      var sb = new StringBuilder();

      if (referencedSymbol is
          { Name: "ValueTuple", FullyQualifiedNamespace: "System" }) {
        sb.Append("(");

        var genericArguments = referencedSymbol.GetTupleElements().ToArray();
        for (var i = 0; i < genericArguments.Length; ++i) {
          if (i > 0) {
            sb.Append(", ");
          }

          var (tupleItemName, tupleItemType) = genericArguments[i];
          sb.Append(sourceSymbol
                        .GetQualifiedNameFromCurrentSymbol(
                            tupleItemType,
                            convertName,
                            getNamespaceParts));
          if (tupleItemName.Length > 0 && tupleItemName != $"Item{1 + i}") {
            sb.Append(" ");
            sb.Append(tupleItemName);
          }
        }

        sb.Append(")");

        return sb.ToString();
      }

      var currentNamespace
          = sourceSymbol.NamespaceParts.Select(EscapeKeyword).ToArray();
      var referencedNamespace =
          (getNamespaceParts != null
              ? getNamespaceParts(referencedSymbol)
              : referencedSymbol.NamespaceParts)
          .Select(EscapeKeyword)
          .ToArray();

      string mergedNamespaceText;
      if (currentNamespace.Length == 0 && referencedNamespace.Length == 0) {
        mergedNamespaceText = "";
      } else if (currentNamespace.Length == 0) {
        mergedNamespaceText = $"{referencedNamespace!}.";
      } else if (referencedNamespace.Length == 0) {
        mergedNamespaceText = $"{string.Join(".", currentNamespace)}.";
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

      sb.Append(mergedNamespaceText);

      foreach (var container in referencedSymbol.DeclaringTypeNamesDownward) {
        sb.Append(container.EscapeKeyword());
        sb.Append('.');
      }

      sb.Append(convertName != null
                    ? convertName(referencedSymbol)
                    : referencedSymbol.Name.EscapeKeyword());

      var typeArguments = referencedSymbol.GenericArguments.ToArray();
      if (typeArguments.Length > 0) {
        sb.Append("<");
        for (var i = 0; i < typeArguments.Length; ++i) {
          if (i > 0) {
            sb.Append(", ");
          }

          var typeArgument = typeArguments[i];
          sb.Append(sourceSymbol.GetQualifiedNameFromCurrentSymbol(
                        typeArgument,
                        convertName,
                        getNamespaceParts));
        }

        sb.Append(">");
      }

      return sb.ToString();
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

    internal static void GetMemberRelativeToAnother(
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

    public static string EscapeKeyword(this string text)
      => !text.IsKeyword() ? text : $"@{text}";

    public static bool IsKeyword(this string text)
      => text is "as"
                 or "bool"
                 or "char"
                 or "class"
                 or "const"
                 or "double"
                 or "float"
                 or "for"
                 or "foreach"
                 or "in"
                 or "int"
                 or "internal"
                 or "public"
                 or "private"
                 or "protected"
                 or "short"
                 or "static"
                 or "string"
                 or "struct"
                 or "this"
                 or "unmanaged"
                 or "void";
  }
}