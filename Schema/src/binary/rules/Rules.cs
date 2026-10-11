using System;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;


namespace schema.binary;

public static partial class Rules {
  private static int diagnosticId_ = 0;

  private static string GetNextDiagnosticId_() {
    var id = Rules.diagnosticId_++;
    return "SCH" + id.ToString("D3");
  }

  public static DiagnosticDescriptor CreateDiagnosticDescriptor(
      string title,
      string messageFormat)
    => new(Rules.GetNextDiagnosticId_(),
           title,
           messageFormat,
           "BinarySchemaAnalyzer",
           DiagnosticSeverity.Error,
           true);

  public static readonly DiagnosticDescriptor
      ParentBinaryConvertabilityMustSatisfyChild
          = Rules.CreateDiagnosticDescriptor(
              "Parent's binary convertability must satisfy child's",
              "Type '{0}' is defined as a child, but its binary convertability is not satisfied by its parent's. The parent must at least implement the same binary serializable/deserializable interface as the child.");


  public static readonly DiagnosticDescriptor
      ChildTypeCanOnlyBeContainedInParent
          = Rules.CreateDiagnosticDescriptor(
              "Child type can only be contained in parent",
              "Type '{0}' is defined as a child of a different type than the one it is contained in.");

  public static readonly DiagnosticDescriptor FormatOnNonNumber
      = Rules.CreateDiagnosticDescriptor(
          "Format attribute on non-numerical member",
          "A Format attribute is applied to the non-numerical member '{0}', which is unsupported.");

  public static DiagnosticDescriptor
      ContainerMemberBinaryConvertabilityNeedsToSatisfyParent { get; } =
    Rules.CreateDiagnosticDescriptor(
        "Container member must satisfy parent's binary convertability",
        "Container member '{0}' must implement at least the same binary serializable/deserializable interface as its parent.");

  public static DiagnosticDescriptor
      ElementBinaryConvertabilityNeedsToSatisfyParent { get; } =
    Rules.CreateDiagnosticDescriptor(
        "Element must satisfy parent's binary convertability",
        "Element of '{0}' must implement at least the same binary serializable/deserializable interface as its parent.");


  public static DiagnosticDescriptor NotSupported { get; }
    = Rules.CreateDiagnosticDescriptor(
        "Not supported",
        "This feature is not yet supported.");

  public static DiagnosticDescriptor UnexpectedAttribute { get; }
    = Rules.CreateDiagnosticDescriptor(
        "Unexpected attribute",
        "Did not expect this attribute on this field.");

  public static readonly DiagnosticDescriptor UnsupportedArrayType
      = Rules.CreateDiagnosticDescriptor(
          "Unsupported array type",
          "Array type '{0}' is not currently supported.");

  public static DiagnosticDescriptor Exception { get; }
    = Rules.CreateDiagnosticDescriptor(
        "Exception",
        "Ran into an exception while generating source ({0}),{1}");

  public static DiagnosticDescriptor SymbolException { get; }
    = Rules.CreateDiagnosticDescriptor(
        "Exception",
        "Ran into an exception while parsing ({0}),{1}");


  public static Diagnostic CreateDiagnostic(
      ISymbol symbol,
      DiagnosticDescriptor descriptor)
    => Diagnostic.Create(
        descriptor,
        symbol.Locations.First(),
        symbol.Name);

  public static void ReportDiagnostic(
      SyntaxNodeAnalysisContext? context,
      ISymbol symbol,
      DiagnosticDescriptor descriptor)
    => context?.ReportDiagnostic(
        Rules.CreateDiagnostic(symbol, descriptor));

  public static Diagnostic CreateExceptionDiagnostic(
      ISymbol symbol,
      Exception exception)
    => Diagnostic.Create(
        Rules.SymbolException,
        symbol.Locations.First(),
        exception.Message,
        exception.StackTrace.Replace("\r\n", "").Replace("\n", ""));

  public static Diagnostic CreateExceptionDiagnostic(
      Exception exception)
    => Diagnostic.Create(
        Rules.Exception,
        null,
        exception.Message,
        exception.StackTrace.Replace("\r\n", "").Replace("\n", ""));


  public static void ReportExceptionDiagnostic(
      SyntaxNodeAnalysisContext? context,
      ISymbol symbol,
      Exception exception)
    => context?.ReportDiagnostic(
        Rules.CreateExceptionDiagnostic(symbol, exception));
}