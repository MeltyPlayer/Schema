using System;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using schema.binary;
using schema.binary.text;

namespace schema.defaultinterface {
  public class IncludeDefaultInterfaceMethodsWriter {
    public string Generate(DefaultInterfaceMethodsData data) {
      var typeSymbol = data.StructureSymbol;

      var typeNamespace = SymbolTypeUtil.MergeContainingNamespaces(typeSymbol);

      var declaringTypes =
          SymbolTypeUtil.GetDeclaringTypesDownward(typeSymbol);

      var sb = new StringBuilder();
      var cbsb = new CurlyBracketTextWriter(new StringWriter(sb));

      // Using statements
      if (data.AllUsingDirectives.Count > 0) {
        foreach (var usingDirective in data.AllUsingDirectives) {
          cbsb.WriteLine($"using {usingDirective.Name};");
        }

        cbsb.WriteLine("");
      }

      // TODO: Handle fancier cases here
      if (typeNamespace != null) {
        cbsb.EnterBlock($"namespace {typeNamespace}");
      }

      foreach (var declaringType in declaringTypes) {
        cbsb.EnterBlock(SymbolTypeUtil.GetQualifiersAndNameFor(declaringType));
      }

      cbsb.EnterBlock(SymbolTypeUtil.GetQualifiersAndNameFor(typeSymbol));

      foreach (var member in data.AllMembersToInclude) {
        cbsb.Write(GetNonGenericText_(typeSymbol, member)
                   .Replace("\r\n", "\n")
                   .Replace("  ", ""));
      }

      // type
      cbsb.ExitBlock();

      // parent types
      foreach (var _ in declaringTypes) {
        cbsb.ExitBlock();
      }

      // namespace
      if (typeNamespace != null) {
        cbsb.ExitBlock();
      }

      return sb.ToString();
    }

    private string GetNonGenericText_(INamedTypeSymbol thisSymbol, ISymbol symbol) {
      var sb = new StringBuilder();

      var containingType = symbol.ContainingType;
      var typeParameters = containingType.TypeParameters;
      var typeArguments = containingType.TypeArguments;
      var typeMap = typeParameters
                    .Zip(typeArguments,
                         (p, a) => (p.ToDisplayString(), a))
                    .ToDictionary(t => t.Item1, t => t.Item2);

      var syntax = symbol.DeclaringSyntaxReferences[0].GetSyntax();
      var tokens = syntax.DescendantTokens().ToArray();

      int indexAfterMemberName = -1;
      for (var i = 0; i < tokens.Length; ++i) {
        var token = tokens[i];
        if (token.IsKind(SyntaxKind.EqualsGreaterThanToken) ||
            token.IsKind(SyntaxKind.OpenBraceToken) ||
            token.IsKind(SyntaxKind.OpenParenToken)) {
          indexAfterMemberName = i;
          break;
        }
      }

      (int, int)? ignoreParentInterfaceRange = null;
      if (indexAfterMemberName > -1) {
        var endOfRange = indexAfterMemberName - 2;
        var startOfRange = endOfRange;
        for (var i = endOfRange; i >= 1;) {
          var expectedDotToken = tokens[i];

          if (expectedDotToken.IsKind(SyntaxKind.DotToken)) {
            var expectedEndOfIdentifierToken = tokens[i - 1];
            if (expectedEndOfIdentifierToken.IsKind(
                    SyntaxKind.IdentifierToken)) {
              startOfRange -= 2;
              i -= 2;
              continue;
            }

            var arrowIndent = 0;
            for (var a = i; a >= 1; --a) {
              var aToken = tokens[a];
              if (aToken.IsKind(SyntaxKind.GreaterThanToken)) {
                ++arrowIndent;
              } else if (aToken.IsKind(SyntaxKind.LessThanToken)) {
                --arrowIndent;
              } else if (arrowIndent == 0 &&
                         aToken.IsKind(SyntaxKind.IdentifierToken)) {
                i = startOfRange = a;
                goto FoundStartOfIdentifier;
              }
            }

            throw new NotImplementedException();
          } else {
            break;
          }

          FoundStartOfIdentifier: ;
        }

        if (endOfRange != startOfRange) {
          ignoreParentInterfaceRange = (startOfRange, endOfRange);
        }
      }

      var hasPrintedPublic = false;
      var squareBracketIndent = 0;
      for (var i = 0; i < tokens.Length; ++i) {
        var token = tokens[i];

        var didUseLeadingTrivia = false;

        if (!hasPrintedPublic) {
          if (token.IsKind(SyntaxKind.OpenBracketToken)) {
            ++squareBracketIndent;
          } else if (token.IsKind(SyntaxKind.CloseBracketToken)) {
            --squareBracketIndent;
          } else if (squareBracketIndent == 0) {
            hasPrintedPublic = true;
            didUseLeadingTrivia = true;

            foreach (var leadingTrivia in token.LeadingTrivia) {
              sb.Append(leadingTrivia.ToFullString());
            }
            sb.Append("public ");
          }
        } else {
          if (ignoreParentInterfaceRange != null) {
            if (i >= ignoreParentInterfaceRange.Value.Item1 &&
                i <= ignoreParentInterfaceRange.Value.Item2) {
              continue;
            }
          }
        }

        var justTokenText = token.ToString();
        string tokenAndSpaces;
        if (!didUseLeadingTrivia) {
          tokenAndSpaces = token.ToFullString();
        } else {
          tokenAndSpaces = justTokenText;
          foreach (var trailingTrivia in token.TrailingTrivia) {
            tokenAndSpaces += trailingTrivia.ToFullString();
          }
        }

        string tokenToWrite;
        if (!typeMap.TryGetValue(justTokenText, out var match)) {
          tokenToWrite = tokenAndSpaces;
        } else {
          var qualifiedName =
              SymbolTypeUtil.GetQualifiedNameFromCurrentSymbol(
                  thisSymbol,
                  match);
          tokenToWrite = tokenAndSpaces.Replace(justTokenText, qualifiedName);
        }

        sb.Append(tokenToWrite);
      }

      return sb.ToString();
    }
  }
}