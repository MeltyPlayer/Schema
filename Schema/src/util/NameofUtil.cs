using System;
using System.Linq;

using Microsoft.CodeAnalysis;

using schema.binary;

namespace schema.util {
  public static class NameofUtil {
    private struct TypeAndNamespace {
      public bool Defined { get; set; }
      public string Name { get; set; }
      public string[] NamespaceParts { get; set; }
    }

    public static string GetChainedAccessFromCallerArgumentExpression(
        string text)
      => NameofUtil.GetChainedAccessFromCallerArgumentExpression_(
          new TypeAndNamespace { Defined = false },
          text);

    public static string GetChainedAccessFromCallerArgumentExpression(
        Type parent,
        string text)
      => NameofUtil.GetChainedAccessFromCallerArgumentExpression_(
          new TypeAndNamespace {
              Defined = true,
              Name = parent.Name,
              NamespaceParts =
                  parent.Namespace?.Split('.') ?? Array.Empty<string>(),
          },
          text);

    public static string GetChainedAccessFromCallerArgumentExpression(
        ISymbol parent,
        string text)
      => NameofUtil.GetChainedAccessFromCallerArgumentExpression_(
          new TypeAndNamespace {
              Defined = true,
              Name = parent.Name,
              NamespaceParts = parent.GetContainingNamespaces() ??
                               Array.Empty<string>(),
          },
          text);


    private static string GetChainedAccessFromCallerArgumentExpression_(
        TypeAndNamespace parent,
        string text) {
      var textLength = text.Length;
      var lastChar = text[textLength - 1];

      if (NameofUtil.GetChainedAccessFromNameof_(
              parent,
              text,
              out var outText)) {
        return outText;
      }

      if (text[0] == '"' && lastChar == '"') {
        return text.Substring(1, textLength - 2);
      }

      return text;
    }

    private static bool GetChainedAccessFromNameof_(
        TypeAndNamespace parent,
        string text,
        out string outText) {
      var nameofText = "nameof(";
      var textLength = text.Length;
      var lastChar = text[textLength - 1];

      if (!text.StartsWith(nameofText) || lastChar != ')') {
        outText = default;
        return false;
      }

      var nameofLength = nameofText.Length;
      outText = text.Substring(nameofLength, textLength - 1 - nameofLength);

      var thisAccessor = "this.";
      if (outText.StartsWith(thisAccessor)) {
        outText = outText.Substring(thisAccessor.Length);
        return true;
      }

      if (parent.Defined) {
        var nameAccessor = $"{parent.Name}.";
        if (outText.StartsWith(nameAccessor)) {
          outText = outText.Substring(nameAccessor.Length);
          return true;
        }

        var namespaceParts = parent.NamespaceParts;
        for (var i = 0; i < namespaceParts.Length; i++) {
          var partialNamespace = string.Join(".", namespaceParts.Skip(i));
          var qualifiedNameAccessor = $"{partialNamespace}.{parent.Name}.";
          if (outText.StartsWith(qualifiedNameAccessor)) {
            outText = outText.Substring(qualifiedNameAccessor.Length);
            return true;
          }
        }
      }

      return true;
    }
  }
}