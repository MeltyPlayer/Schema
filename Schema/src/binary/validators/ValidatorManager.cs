using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

using Microsoft.CodeAnalysis;

using schema.util.types;

namespace schema.binary.validators;

public static class ValidatorManager {
  public static IReadOnlyList<Type> AllValidatorTypes { get; }
  public static IReadOnlyList<IValidator> AllValidators { get; }

  public static IReadOnlyList<IContainerValidator> AllContainerValidators {
    get;
  }

  public static IReadOnlyList<IMemberValidator> AllMemberValidators { get; }

  public static ImmutableArray<DiagnosticDescriptor> AllDiagnosticDescriptors {
    get;
  }

  static ValidatorManager() {
    AllValidatorTypes = [.. IValidator.AllImplementations()];
    AllValidators = [
        .. AllValidatorTypes
            .Select(t => (IValidator) t.GetConstructor([]).Invoke([]))
    ];
    AllContainerValidators = [.. AllValidators.OfType<IContainerValidator>()];
    AllMemberValidators = [.. AllValidators.OfType<IMemberValidator>()];

    AllDiagnosticDescriptors
        = ValidatorManager
          .AllValidatorTypes
          .SelectMany(t => t.GetFields(
                                BindingFlags.Static |
                                BindingFlags.Public |
                                BindingFlags.NonPublic)
                            .Where(f => f.FieldType ==
                                        typeof(Rule)))
          .OrderBy(f => f.Name)
          .Select(f => (Rule) f.GetValue(null))
          .Select(rule => {
                    rule.InitDescriptor();
                    return rule.DiagnosticDescriptor;
                  })
          .ToImmutableArray();
  }


  public static void EnsureInitialized() { }
}