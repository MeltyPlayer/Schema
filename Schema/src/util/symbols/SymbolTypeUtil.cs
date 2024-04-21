using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Microsoft.CodeAnalysis;

using schema.binary;
using schema.binary.parser;
using schema.util.asserts;
using schema.util.diagnostics;
using schema.util.types;


namespace schema.util.symbols {
  public static class SymbolTypeUtil {
    public static INamedTypeSymbol[] GetDeclaringTypesDownward(
        this ISymbol type) {
      var declaringTypes = new List<INamedTypeSymbol>();

      var declaringType = type.ContainingType;
      while (declaringType != null) {
        declaringTypes.Add(declaringType);
        declaringType = declaringType.ContainingType;
      }

      declaringTypes.Reverse();

      return declaringTypes.ToArray();
    }

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

    public static StringBuilder AppendGenericArgumentsFor(
        this StringBuilder sb,
        ITypeSymbol sourceSymbol,
        ITypeSymbol referencedSymbol,
        Func<ITypeSymbol, ISymbol?, string>? convertName = null,
        Func<ITypeSymbol, IEnumerable<string>>? getNamespaceParts = null) {
      if (referencedSymbol.IsGeneric(out var typeParameters,
                                     out var typeArguments)) {
        sb.Append("<");
        for (var i = 0; i < typeArguments.Length; ++i) {
          if (i > 0) {
            sb.Append(", ");
          }

          var typeArgument = typeArguments[i];
          sb.Append(sourceSymbol.GetQualifiedNameFromCurrentSymbol(
                        typeArgument,
                        typeParameters[i],
                        convertName,
                        getNamespaceParts));
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
        this ITypeSymbol sourceSymbol,
        ITypeSymbol referencedSymbol)
      => sourceSymbol.GetQualifiedNameFromCurrentSymbol(referencedSymbol, null);

    public static string GetQualifiedNameFromCurrentSymbol(
        this ITypeSymbol sourceSymbol,
        ITypeSymbol referencedSymbol,
        ISymbol? symbolForChecks,
        Func<ITypeSymbol, ISymbol?, string>? convertName = null,
        Func<ITypeSymbol, IEnumerable<string>>? getNamespaceParts = null) {
      if (referencedSymbol.IsArray(out var elementType)) {
        return
            $"{sourceSymbol.GetQualifiedNameFromCurrentSymbol(elementType, null, convertName, getNamespaceParts)}[]";
      }

      if (referencedSymbol.IsNullable()) {
        if (referencedSymbol.IsType(typeof(Nullable<>))) {
          var referencedNamedTypeSymbol
              = Asserts.AsA<INamedTypeSymbol>(referencedSymbol);
          var typeArgument = referencedNamedTypeSymbol.TypeArguments.Single();
          var typeParameter = referencedNamedTypeSymbol.TypeParameters.Single();

          return
              $"{sourceSymbol.GetQualifiedNameFromCurrentSymbol(typeArgument, typeParameter, convertName, getNamespaceParts)}?";
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

      if (referencedSymbol.IsString()) {
        return "string";
      }

      if (referencedSymbol.IsGenericTypeParameter(out _)) {
        return referencedSymbol.Name.EscapeKeyword();
      }

      if (referencedSymbol.IsType(typeof(void))) {
        return "void";
      }

      var sb = new StringBuilder();

      if (referencedSymbol.IsTuple(out var tupleElements)) {
        sb.Append("(");

        var genericArguments = tupleElements.ToArray();
        for (var i = 0; i < genericArguments.Length; ++i) {
          if (i > 0) {
            sb.Append(", ");
          }

          var tupleElement = genericArguments[i];
          var tupleItemName = tupleElement.Name;
          var tupleItemType = tupleElement.Type;
          sb.Append(sourceSymbol
                        .GetQualifiedNameFromCurrentSymbol(
                            tupleItemType,
                            null,
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
          = sourceSymbol.GetContainingNamespaces()
                        .Select(EscapeKeyword)
                        .ToArray();
      var referencedNamespace =
          (getNamespaceParts != null
              ? getNamespaceParts(referencedSymbol)
              : referencedSymbol.GetContainingNamespaces())
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

      foreach (var container in referencedSymbol.GetDeclaringTypesDownward()) {
        sb.Append(container.Name.EscapeKeyword());
        sb.Append('.');
      }

      sb.Append(convertName != null
                    ? convertName(referencedSymbol, symbolForChecks)
                    : referencedSymbol.Name.EscapeKeyword());

      sb.AppendGenericArgumentsFor(sourceSymbol,
                                   referencedSymbol,
                                   convertName,
                                   getNamespaceParts);

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