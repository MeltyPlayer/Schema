using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;


namespace schema.binary {
  public static partial class Rules {
    private static int diagnosticId_ = 0;

    private static string GetNextDiagnosticId_() {
      var id = Rules.diagnosticId_++;
      return "SCH" + id.ToString("D3");
    }

    private static DiagnosticDescriptor CreateDiagnosticDescriptor_(
        string title,
        string messageFormat)
      => new(Rules.GetNextDiagnosticId_(),
             title,
             messageFormat,
             "BinarySchemaAnalyzer",
             DiagnosticSeverity.Error,
             true);


    public static readonly DiagnosticDescriptor SchemaTypeMustBePartial
        = Rules.CreateDiagnosticDescriptor_(
            "Schema type must be partial",
            "Schema type '{0}' must be partial to accept automatically generated read/write code.");

    public static readonly DiagnosticDescriptor ContainerTypeMustBePartial
        = Rules.CreateDiagnosticDescriptor_(
            "Container of schema type must be partial",
            "Type '{0}' contains a schema type, must be partial to accept automatically generated code.");

    public static readonly DiagnosticDescriptor ChildTypeMustBeContainedInParent
        = Rules.CreateDiagnosticDescriptor_(
            "Child type must be contained in parent",
            "Type '{0}' is defined as a child, but is not actually contained in its parent type.");

    public static readonly DiagnosticDescriptor
        ChildTypeCanOnlyBeContainedInParent
            = Rules.CreateDiagnosticDescriptor_(
                "Child type can only be contained in parent",
                "Type '{0}' is defined as a child of a different type than the one it is contained in.");

    public static readonly DiagnosticDescriptor MutableStringNeedsLengthSource
        = Rules.CreateDiagnosticDescriptor_(
            "Schema string must have length source",
            "Mutable string '{0}' is missing a LengthSource attribute.");

    public static readonly DiagnosticDescriptor MutableArrayNeedsLengthSource
        = Rules.CreateDiagnosticDescriptor_(
            "Mutable array needs length source",
            "Mutable array '{0}' is missing a LengthSource attribute.");

    public static readonly DiagnosticDescriptor FormatOnNonNumber
        = Rules.CreateDiagnosticDescriptor_(
            "Format attribute on non-numerical member",
            "A Format attribute is applied to the non-numerical member '{0}', which is unsupported.");

    public static DiagnosticDescriptor EnumNeedsIntegerFormat { get; }
      = Rules.CreateDiagnosticDescriptor_(
          "Enum needs integer format",
          "Enum member '{0}' needs either a valid IntegerFormat attribute or for its enum type to specify an underlying representation.");

    public static DiagnosticDescriptor BooleanNeedsIntegerFormat { get; }
      = Rules.CreateDiagnosticDescriptor_(
          "Boolean needs integer format",
          "Boolean member '{0}' needs a valid IntegerFormat attribute.");

    public static DiagnosticDescriptor IfBooleanNeedsNullable { get; }
      = Rules.CreateDiagnosticDescriptor_(
          "IfBoolean Attribute needs nullable type",
          "Member '{0}' must be a nullable type to use IfBoolean.");

    public static DiagnosticDescriptor
        StructureMemberNeedsToImplementIBiSerializable { get; } =
      Rules.CreateDiagnosticDescriptor_(
          "Structure member needs to implement IBinaryConvertible",
          "Structure member '{0}' must implement IBinaryConvertible.");

    public static DiagnosticDescriptor
        ElementNeedsToImplementIBiSerializable { get; } =
      Rules.CreateDiagnosticDescriptor_(
          "Element needs to implement IBinaryConvertible",
          "Element of '{0}' must implement IBinaryConvertible.");


    public static readonly DiagnosticDescriptor ConstUninitialized
        = Rules.CreateDiagnosticDescriptor_(
            "Const uninitialized",
            "Const member '{0}' must be initialized.");

    public static DiagnosticDescriptor NotSupported { get; }
      = Rules.CreateDiagnosticDescriptor_(
          "Not supported",
          "This feature is not yet supported.");

    public static readonly DiagnosticDescriptor ReadAlreadyDefined
        = Rules.CreateDiagnosticDescriptor_(
            "Read already defined",
            "A Read method for '{0}' was already defined.");

    public static readonly DiagnosticDescriptor WriteAlreadyDefined
        = Rules.CreateDiagnosticDescriptor_(
            "Write already defined",
            "A Write method for '{0}' was already defined.");

    public static DiagnosticDescriptor UnexpectedAttribute { get; }
      = Rules.CreateDiagnosticDescriptor_(
          "Unexpected attribute",
          "Did not expect this attribute on this field.");

    public static readonly DiagnosticDescriptor UnsupportedArrayType
        = Rules.CreateDiagnosticDescriptor_(
            "Unsupported array type",
            "Array type '{0}' is not currently supported.");

    public static DiagnosticDescriptor Exception { get; }
      = Rules.CreateDiagnosticDescriptor_(
          "Exception",
          "Ran into an exception while parsing ({0}),{1}");


    public static void ReportDiagnostic(
        SyntaxNodeAnalysisContext? context,
        Diagnostic diagnostic)
      => context?.ReportDiagnostic(diagnostic);

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
        Exception exception)
      => Diagnostic.Create(
          Rules.Exception,
          null,
          exception.ToString());

    public static void ReportExceptionDiagnostic(
        SyntaxNodeAnalysisContext? context,
        Exception exception)
      => context?.ReportDiagnostic(Rules.CreateExceptionDiagnostic(exception));

    public static Diagnostic CreateExceptionDiagnostic(
        ISymbol symbol,
        Exception exception)
      => Diagnostic.Create(
          Rules.Exception,
          symbol.Locations.First(),
          exception.Message,
          exception.StackTrace.Replace("\r\n", "").Replace("\n", ""));

    public static void ReportExceptionDiagnostic(
        SyntaxNodeAnalysisContext? context,
        ISymbol symbol,
        Exception exception)
      => context?.ReportDiagnostic(
          Rules.CreateExceptionDiagnostic(symbol, exception));
  }
}