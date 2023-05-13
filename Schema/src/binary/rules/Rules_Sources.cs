using Microsoft.CodeAnalysis;


namespace schema.binary {
  public static partial class Rules {
    public static readonly DiagnosticDescriptor DependentMustComeAfterSource
        = Rules.CreateDiagnosticDescriptor_(
            "Field must come after what it is dependent on",
            "Field '{0}' is dependent on another field, and therefore must come after it.");

    public static readonly DiagnosticDescriptor SourceMustBePrivate
        = Rules.CreateDiagnosticDescriptor_(
            "Source must be private",
            "Source field '{0}' must be private, because it will never be used outside of read time.");

    public static readonly DiagnosticDescriptor AllMembersInChainMustUseSchema
        = Rules.CreateDiagnosticDescriptor_(
            "All members in chain must use schema",
            "All members in chain must use schema.");
  }
}