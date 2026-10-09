using System;
using System.Collections.Generic;
using System.Linq;

using schema.util.types;

namespace schema.binary.validators;

internal static class ValidatorManager {
  public static IReadOnlyList<Type> AllValidatorTypes { get; }
  public static IReadOnlyList<IValidator> AllValidators { get; }

  public static IReadOnlyList<IContainerValidator> AllContainerValidators {
    get;
  }

  public static IReadOnlyList<IMemberValidator> AllMemberValidators { get; }

  static ValidatorManager() {
    AllValidatorTypes = [.. IValidator.AllImplementations()];
    AllValidators = [
        .. AllValidatorTypes
            .Select(t => (IValidator) t.GetConstructor([]).Invoke([]))
    ];
    AllContainerValidators = [..AllValidators.OfType<IContainerValidator>()];
    AllMemberValidators = [..AllValidators.OfType<IMemberValidator>()];
  }
}