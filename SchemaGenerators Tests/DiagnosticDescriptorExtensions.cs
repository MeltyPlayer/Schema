using Microsoft.CodeAnalysis;

namespace schema.binary {
  internal static class DiagnosticDescriptorExtensions {
    internal static string ToString(this DiagnosticDescriptor src)
      => src.MessageFormat.ToString();
  }
}