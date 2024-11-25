using System;

using Microsoft.CodeAnalysis;

using schema.util.symbols;


namespace schema.util.text;

public static class SourceWriterSymbolExtensions {
  public static void WriteNamespaceAndParentTypeBlocks(
      this ISourceWriter sw,
      INamedTypeSymbol symbol,
      Action insideBlockHandler) {
    var fullyQualifiedNamespace = symbol.GetFullyQualifiedNamespace();
    if (fullyQualifiedNamespace != null) {
      sw.WriteLine($"namespace {fullyQualifiedNamespace};")
        .WriteLine();
    }

    var declaringTypes = symbol.GetDeclaringTypesDownward();
    foreach (var declaringType in declaringTypes) {
      sw.EnterBlock(declaringType
                        .GetQualifiersAndNameAndGenericParametersFor());
    }

    insideBlockHandler();

    // parent types
    foreach (var _ in declaringTypes) {
      sw.ExitBlock();
    }
  }
}